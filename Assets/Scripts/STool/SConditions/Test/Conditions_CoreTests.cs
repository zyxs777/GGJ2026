// #nullable enable
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using NUnit.Framework;
// using STool.SConditions.CommonCond;
//
// namespace STool.SConditions.Test
// {
//     /// <summary>
//     ///     测试范围：SConditions（独立，不依赖黑板）
//     ///     说明：
//     ///     - 为了适配你当前 CommonCond 的具体实现，本测试对组合条件字段名不做强假设，
//     ///     会用反射把子条件塞进 And/Or，把 inner 塞进 Not。
//     ///     - TimeWithin 如你当前项目未实现，可通过 define 开关跳过编译。
//     /// </summary>
//     public class ConditionsCoreTests
//     {
//         private ValueKey<float> _health;
//         private ValueKey<bool> _inCombat;
//
//         private sealed class DictValueSource : IValueSource
//         {
//             private readonly Dictionary<string, object> _data = new();
//
//             public void Set<T>(ValueKey<T> key, T value)
//             {
//                 _data[key.Name] = value!;
//             }
//
//             public bool TryGet<T>(ValueKey<T> key, out T value)
//             {
//                 if (_data.TryGetValue(key.Name, out var obj) && obj is T t)
//                 {
//                     value = t;
//                     return true;
//                 }
//
//                 value = default!;
//                 return false;
//             }
//         }
//
//         [SetUp]
//         public void SetUp()
//         {
//             _health = new ValueKey<float>("Health", 100f);
//             _inCombat = new ValueKey<bool>("InCombat");
//         }
//
//         [Test]
//         public void ValueKey_HoldsNameAndDefaultValue()
//         {
//             Assert.AreEqual("Health", _health.Name);
//             Assert.AreEqual(100f, _health.DefaultValue, 0.0001f);
//
//             Assert.AreEqual("InCombat", _inCombat.Name);
//             Assert.AreEqual(false, _inCombat.DefaultValue);
//         }
//
//         [Test]
//         public void TimeReached_EvaluatesByContextNow()
//         {
//             var source = new DictValueSource();
//             var cond = new TimeReached { At = 3f };
//
//             Assert.False(cond.Evaluate(new ConditionContext(source, 2.99f)));
//             Assert.True(cond.Evaluate(new ConditionContext(source, 3.00f)));
//             Assert.True(cond.Evaluate(new ConditionContext(source, 100f)));
//         }
//
// #if SCONDITIONS_HAS_TIMEWITHIN
//         [Test]
//         public void TimeWithin_EvaluatesInclusiveRange()
//         {
//             var source = new DictValueSource();
//             var cond = new TimeWithin();
//             SetMember(cond, "Start", 2f);
//             SetMember(cond, "End", 5f);
//
//             Assert.False(cond.Evaluate(new ConditionContext(source, 1.99f)));
//             Assert.True(cond.Evaluate(new ConditionContext(source, 2.00f)));
//             Assert.True(cond.Evaluate(new ConditionContext(source, 3.00f)));
//             Assert.True(cond.Evaluate(new ConditionContext(source, 5.00f)));
//             Assert.False(cond.Evaluate(new ConditionContext(source, 5.01f)));
//         }
// #endif
//
//         [Test]
//         public void FloatLessThan_UsesSourceValue_WhenPresent()
//         {
//             var source = new DictValueSource();
//             source.Set(_health, 20f);
//
//             var cond = new FloatLessThan { Key = _health, Threshold = 30f };
//             Assert.True(cond.Evaluate(new ConditionContext(source, 0)));
//         }
//
//         [Test]
//         public void FloatLessThan_FallsBackToDefault_WhenMissing()
//         {
//             var source = new DictValueSource();
//             var cond = new FloatLessThan { Key = _health, Threshold = 30f };
//
//             // default health = 100 => 100 < 30 is false
//             Assert.False(cond.Evaluate(new ConditionContext(source, 0)));
//         }
//
//         [Test]
//         public void BoolIsTrue_UsesSourceValue_AndDefaultFallback()
//         {
//             var source = new DictValueSource();
//             var cond = new BoolIsTrue { Key = _inCombat };
//
//             Assert.False(cond.Evaluate(new ConditionContext(source, 0)));
//
//             source.Set(_inCombat, true);
//             Assert.True(cond.Evaluate(new ConditionContext(source, 0)));
//         }
//
//         [Test]
//         public void Composite_AndCondition_AllMustPass()
//         {
//             var source = new DictValueSource();
//             source.Set(_health, 20f);
//             source.Set(_inCombat, true);
//
//             var and = new AndCondition();
//             AddChildren(and,
//                 new FloatLessThan { Key = _health, Threshold = 30f },
//                 new BoolIsTrue { Key = _inCombat },
//                 new TimeReached { At = 1f }
//             );
//
//             Assert.False(and.Evaluate(new ConditionContext(source, 0.5f)));
//             Assert.True(and.Evaluate(new ConditionContext(source, 1.0f)));
//         }
//
//         [Test]
//         public void Composite_OrCondition_AnyPasses()
//         {
//             var source = new DictValueSource();
//             source.Set(_health, 80f);
//             source.Set(_inCombat, false);
//
//             var or = new OrCondition();
//             AddChildren(or,
//                 new FloatLessThan { Key = _health, Threshold = 30f }, // false
//                 new BoolIsTrue { Key = _inCombat }, // false
//                 new TimeReached { At = 0f } // true
//             );
//
//             Assert.True(or.Evaluate(new ConditionContext(source, 0)));
//         }
//
//         [Test]
//         public void Composite_NotCondition_NegatesInner()
//         {
//             var source = new DictValueSource();
//
//             var not1 = new NotCondition();
//             SetInner(not1, new TimeReached { At = 10f });
//
//             Assert.True(not1.Evaluate(new ConditionContext(source, 0)));
//             Assert.False(not1.Evaluate(new ConditionContext(source, 10f)));
//
//             var notNull = new NotCondition();
//             SetInner(notNull, null);
//             Assert.True(notNull.Evaluate(new ConditionContext(source, 0)));
//         }
//
//         // ---------- Reflection helpers (make tests resilient to field naming) ----------
//
//         private static void AddChildren(object composite, params ICondition[] children)
//         {
//             // Find List<ICondition> field/property and add children
//             var type = composite.GetType();
//
//             // fields
//             foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//                 if (TryAddToListMember(f.FieldType, f.GetValue(composite), children,
//                         v => f.SetValue(composite, v)))
//                     return;
//
//             // properties
//             foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//             {
//                 if (!p.CanRead) continue;
//                 var val = p.GetValue(composite);
//                 if (TryAddToListMember(p.PropertyType, val, children, v =>
//                     {
//                         if (p.CanWrite) p.SetValue(composite, v);
//                     }))
//                     return;
//             }
//
//             Assert.Fail($"Could not find a List<ICondition> member on {type.Name} to add children.");
//         }
//
//         private static bool TryAddToListMember(Type memberType, object? memberValue,
//             ICondition[] children, Action<object?> assign)
//         {
//             switch (memberValue)
//             {
//                 // We accept List<ICondition> or any IList that can hold ICondition
//                 case IList list:
//                 {
//                     foreach (var c in children) list.Add(c);
//                     return true;
//                 }
//                 // If null and type looks like List<ICondition>, create it
//                 case null when typeof(IList).IsAssignableFrom(memberType):
//                 {
//                     var created = Activator.CreateInstance(memberType);
//                     if (created is IList createdList)
//                     {
//                         foreach (var c in children) createdList.Add(c);
//                         assign(created);
//                         return true;
//                     }
//
//                     break;
//                 }
//             }
//
//             return false;
//         }
//
//         private static void SetInner(object notCondition, ICondition? inner)
//         {
//             var type = notCondition.GetType();
//
//             foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//                 if (typeof(ICondition).IsAssignableFrom(f.FieldType))
//                 {
//                     f.SetValue(notCondition, inner);
//                     return;
//                 }
//
//             foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
//                 if (p.CanWrite && typeof(ICondition).IsAssignableFrom(p.PropertyType))
//                 {
//                     p.SetValue(notCondition, inner);
//                     return;
//                 }
//
//             Assert.Fail($"Could not find an ICondition member on {type.Name} to set inner.");
//         }
//     }
// }