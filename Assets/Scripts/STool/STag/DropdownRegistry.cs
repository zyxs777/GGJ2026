#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace STool.STag
{
    public static class DropdownRegistry
    {
        private static readonly Dictionary<string, List<(string label, object value)>> _cache = new();
        private static bool _built;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Build();
            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                _built = false;
                _cache.Clear();
                Build();
            };
            Debug.Log($"{nameof(DropdownRegistry)}.{nameof(Init)}=>{_cache.Count}");
        }
        private static void BuildIfNeeded()
        {
            if (!_built) Build();
        }
        private static void Build()
        {
            _built = true;

            // 关键：扫描“成员”，不是扫描“类型”
            foreach (var field in TypeCache.GetFieldsWithAttribute<DropdownItemAttribute>())
                CollectFromField(field);
        
            foreach (var method in TypeCache.GetMethodsWithAttribute<DropdownItemAttribute>())
                CollectFromMethod(method);
        }
        private static void CollectFromField(FieldInfo field)
        {
            if (!field.IsStatic) return;

            var attrs = field.GetCustomAttributes<DropdownItemAttribute>(false);
            var value = field.GetValue(null);

            foreach (var a in attrs)
                Add(a, field.Name, value);
        }
        private static void CollectFromProperty(PropertyInfo prop)
        {
            var get = prop.GetGetMethod(true);
            if (get == null || !get.IsStatic) return;

            var attrs = prop.GetCustomAttributes<DropdownItemAttribute>(false);
            object value;
            try { value = prop.GetValue(null); } catch { return; }

            foreach (var a in attrs)
                Add(a, prop.Name, value);
        }
        private static void CollectFromMethod(MethodInfo method)
        {
            if (!method.IsStatic) return;
            if (method.GetParameters().Length != 0) return;

            var attrs = method.GetCustomAttributes<DropdownItemAttribute>(false);
            object value;
            try { value = method.Invoke(null, null); } catch { return; }

            // 方法返回 IEnumerable（非 string）就展开
            if (value is IEnumerable e && value is not string)
            {
                foreach (var a in attrs)
                foreach (var one in e)
                    Add(a, method.Name, one);
            }
            else
            {
                foreach (var a in attrs)
                    Add(a, method.Name, value);
            }
        }
        private static void Add(DropdownItemAttribute a, string memberName, object value)
        {
            if (value == null) return;

            var label = string.IsNullOrEmpty(a.Label) ? memberName : a.Label;

            if (!_cache.TryGetValue(a.Group, out var list))
                _cache[a.Group] = list = new List<(string, object)>();

            list.Add((label, value));
        }
        
        
        public static IEnumerable<(string label, T value)> Get<T>(string group)
        {
            BuildIfNeeded();
            if (!_cache.TryGetValue(group, out var list)) yield break;

            foreach (var (label, value) in list)
                if (value is T t) yield return (label, t);
        }
        public static IReadOnlyDictionary<string, List<(string label, object value)>> GetAllDropDowns() => _cache;
    }
}
#endif
