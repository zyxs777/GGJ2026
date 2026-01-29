// using NUnit.Framework;
// using STool.SBlackBoard;
// using STool.SConditions.BlackboardAdapter.CommonCond;
//
// namespace STool.SConditions.BlackboardAdapter.Test
// {
//     /// <summary>
//     /// 测试范围：BlackboardAdapter（将 IBlackboard 适配为 IValueSource）
//     /// 覆盖点：
//     /// - KeyRegistry 映射成功
//     /// - BlackboardValueSource 能通过映射读到黑板值
//     /// - 未注册映射时 TryGet 返回 false（条件应走 DefaultValue 兜底）
//     /// - 值类型读取路径不发生装箱（这里更偏行为测试；性能/GC 属黑板侧）
//     /// </summary>
//     public class BlackboardAdapterTests
//     {
//         private ValueKey<float> CK_Health;
//         private BBKey<float> BB_Health;
//
//         [SetUp]
//         public void SetUp()
//         {
//             CK_Health = new ValueKey<float>("Health", 100f);
//             BB_Health = new BBKey<float>("Health", 100f);
//         }
//
//         [Test]
//         public void KeyRegistry_RegisterAndResolve_Works()
//         {
//             // 测试内容：注册后可以 Resolve 到对应 BBKey<T>
//             var registry = new KeyRegistry();
//             registry.Register(CK_Health, BB_Health);
//
//             Assert.True(registry.TryResolve(CK_Health, out BBKey<float> resolved));
//             Assert.AreEqual(BB_Health, resolved);
//         }
//
//         [Test]
//         public void BlackboardValueSource_TryGet_ReadsFromBlackboard_WhenMapped()
//         {
//             // 测试内容：当 ValueKey 与 BBKey 映射存在时，可读取黑板值
//             var bb = new Blackboard();
//             bb.Set(BB_Health, 25f);
//
//             var registry = new KeyRegistry();
//             registry.Register(CK_Health, BB_Health);
//
//             var source = new BlackboardValueSource(bb, registry);
//
//             Assert.True(source.TryGet(CK_Health, out float v));
//             Assert.AreEqual(25f, v, 0.0001f);
//         }
//
//         [Test]
//         public void BlackboardValueSource_TryGet_ReturnsFalse_WhenNotMapped()
//         {
//             // 测试内容：未注册映射时 TryGet 返回 false
//             var bb = new Blackboard();
//             bb.Set(BB_Health, 25f);
//
//             var registry = new KeyRegistry(); // 没注册
//             var source = new BlackboardValueSource(bb, registry);
//
//             Assert.False(source.TryGet(CK_Health, out float _));
//         }
//
//         [Test]
//         public void Condition_UsingAdapter_FallsBackToDefault_WhenNotMapped()
//         {
//             // 测试内容：未映射时，FloatLessThan 会用 ValueKey.DefaultValue 兜底
//             // CK_Health 默认 100，Threshold=30 => false
//             var bb = new Blackboard();
//             var registry = new KeyRegistry(); // 不注册
//             var source = new BlackboardValueSource(bb, registry);
//
//             var cond = new FloatLessThan
//             {
//                 Key = CK_Health,
//                 Threshold = 30f
//             };
//
//             var ctx = new ConditionContext(source, now: 0);
//             Assert.False(cond.Evaluate(in ctx));
//         }
//
//         [Test]
//         public void Condition_UsingAdapter_ReadsActualBlackboardValue_WhenMapped()
//         {
//             // 测试内容：映射存在时，FloatLessThan 应读取黑板实际值并判断
//             var bb = new Blackboard();
//             bb.Set(BB_Health, 20f);
//
//             var registry = new KeyRegistry();
//             registry.Register(CK_Health, BB_Health);
//
//             var source = new BlackboardValueSource(bb, registry);
//
//             var cond = new FloatLessThan
//             {
//                 Key = CK_Health,
//                 Threshold = 30f
//             };
//
//             var ctx = new ConditionContext(source, now: 0);
//             Assert.True(cond.Evaluate(in ctx));
//         }
//     }
// }
