using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.EventExtension
{
    [Serializable]
    public class ReturnMemberCall<T>
    {
        [LabelText("Target")] 
        [SerializeField]
        private UnityEngine.Object target;
        #if UNITY_EDITOR
        [ValueDropdown(nameof(GetMemberOptions))] 
        #endif
        [SerializeField]
        private string memberName; // 方法名 或 get_属性名

        private UnityEngine.Object _formerTarget;
        private string _formerMemberName;
        
        // 缓存成强类型委托，避免频繁反射/装箱
        [NonSerialized] private Func<T> _cached;

        public bool IsValid => target != null && !string.IsNullOrEmpty(memberName);
        public bool TryEvaluate(out T value)
        {
            value = default;
            if (!target || string.IsNullOrEmpty(memberName))
            {
                // Debug.LogWarning($"UnValid Target or Member");
                return false;
            }
            if (_cached == null || (_formerTarget !=  target || _formerMemberName != memberName))
            {
                BindDelegate();
                if (_cached == null)
                {
                    // Debug.LogWarning($"UnMatch Target {target.name}.{memberName}");
                    return false;
                }
            }

            value = _cached();
            return true;
        }
        public T EvaluateOrDefault()
        {
            TryEvaluate(out var v);
            return v;
        }

        #region -------------------- 绑定 --------------------
        private void BindDelegate()
        {
            // Debug.Log($"Method Rebind to {target}.{memberName}");
            var rawTarget = target;
            var type = rawTarget.GetType();

            // —— 解析 memberName 里是否带了组件类型前缀（当 Target 为 GameObject 时）——
            var invokeTarget = rawTarget;
            var member = memberName;

            _cached = null;
            _formerTarget = rawTarget;
            _formerMemberName = member;
            
            // 约定编码：ComponentType|member
            if (rawTarget is GameObject go && memberName.Contains("|"))
            {
                // Debug.Log($"{rawTarget} is GO");
                var split = memberName.Split('|');
                var compTypeName = split[0];
                member = split[1];

                // 尝试用 FullName / Name 解析类型
                var compType = FindTypeByName(compTypeName);
                if (compType != null)
                {
                    var comp = go.GetComponent(compType);
                    if (comp != null)
                    {
                        invokeTarget = comp; // 真正的目标变成该组件
                        type = comp.GetType();
                        // Debug.Log($"Find on {rawTarget} => {compType}");
                    }
                }
            }

            // —— 后面保持不变：member 可能是 方法名 或 get_Prop —— // 方法：T M()
            var mi = type.GetMethod(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (mi != null && mi.GetParameters().Length == 0 && typeof(T).IsAssignableFrom(mi.ReturnType))
            {
                if (mi.IsStatic) _cached = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), null, mi, false) ?? (() => (T)mi.Invoke(null, null));
                else _cached = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), invokeTarget, mi, false) ?? (() => (T)mi.Invoke(invokeTarget, null));
                // return;
            }

            // 属性 getter：get_Prop()
            if (member.StartsWith("get_", StringComparison.Ordinal))
            {
                var prop = type.GetProperty(member[4..], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var getter = prop?.GetGetMethod(true);
                if (getter != null && typeof(T).IsAssignableFrom(prop.PropertyType))
                {
                    if (getter.IsStatic) _cached = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), null, getter, false) ?? (() => (T)getter.Invoke(null, null));
                    else _cached = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), invokeTarget, getter, false) ?? (() => (T)getter.Invoke(invokeTarget, null));
                }
            }

            if (_cached != null) return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogError($"Bind failed: {type.Name}.{member}", rawTarget);
            return;
        }
        private static Type FindTypeByName(string name)
        {
            // 先精确 FullName，再退回短名
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name, throwOnError: false);
                if (t != null) return t;
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == name);
        }
        #endregion

        #region Tool

#if UNITY_EDITOR
        private IEnumerable<ValueDropdownItem<string>> GetMemberOptions()
        {
            // Debug.Log($"Member Options Generating");
            if (!target) return Enumerable.Empty<ValueDropdownItem<string>>();
            // 目标是 GameObject：聚合所有组件 → 组件名/成员
            if (target is GameObject go) return BuildForGameObject(go);
            // 目标是 Component/ScriptableObject：用“类型名/成员”
            return BuildForSingleObject(target, target.GetType().Name);
        }
        private IEnumerable<ValueDropdownItem<string>> BuildForGameObject(GameObject go)
        {
            var items = new List<ValueDropdownItem<string>>();
            foreach (var comp in go.GetComponents<Component>())
            {
                if (!comp) continue;
                var compType = comp.GetType();
                var compName = compType.Name;
                // 方法：返回 T、无参
                items.AddRange(from m in compType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) where m.GetParameters().Length == 0 && typeof(T).IsAssignableFrom(m.ReturnType) let text = $"{compName}/{m.Name}() : {Pretty(m.ReturnType)}" let val = $"{compName}|{m.Name}" select new ValueDropdownItem<string>(text, val));
                // 属性：可读，类型可赋给 T
                items.AddRange(from p in compType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) let g = p.GetGetMethod(true) where g != null && typeof(T).IsAssignableFrom(p.PropertyType) let text = $"{compName}/{p.Name} (property) : {Pretty(p.PropertyType)}" let val = $"{compName}|get_{p.Name}" select new ValueDropdownItem<string>(text, val));
            }

            // 去重、排序
            return items
                .GroupBy(i => i.Value)
                .Select(g => g.First())
                .OrderBy(i => i.Text);
        }
        private IEnumerable<ValueDropdownItem<string>> BuildForSingleObject(UnityEngine.Object obj, string prefix)
        {
            var t = obj.GetType();
            var list = (from m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) where m.GetParameters().Length == 0 && typeof(T).IsAssignableFrom(m.ReturnType) let text = $"{prefix}/{(m.IsStatic ? "static " : "")}{m.Name}() : {Pretty(m.ReturnType)}" select new ValueDropdownItem<string>(text, m.Name)).ToList();
            list.AddRange(from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) let g = p.GetGetMethod(true) where g != null && typeof(T).IsAssignableFrom(p.PropertyType) let text = $"{prefix}/{(g.IsStatic ? "static " : "")}{p.Name} (property) : {Pretty(p.PropertyType)}" select new ValueDropdownItem<string>(text, "get_" + p.Name));

            return list
                .GroupBy(i => i.Value)
                .Select(g => g.First())
                .OrderBy(i => i.Text);
        }

        private static string Pretty(Type t) => t == typeof(float) ? "float" :
            t == typeof(int) ? "int" :
            t == typeof(bool) ? "bool" :
            t.Name;
#endif

        #endregion
    }
}