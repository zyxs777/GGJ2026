using System;
using System.Collections.Generic;
using UnityEngine;

namespace STool.CollectionUtility
{
    /// <summary>
    /// 一个以 string 为 key 的异构容器：每个 key 对应一个强类型槽位 Entry&lt;T&gt;。
    /// - Set/Get 使用泛型，值类型不会发生装箱（Entry&lt;T&gt; 内部用字段 T 存储）。
    /// - ResetDefault 可将所有项重置为各自类型的 default(T)。
    /// </summary>
    public sealed class BucketDictionary
    {
        private Dictionary<string, IBucket> _map = new();

        /// <summary>写入（若 key 已存在，会要求类型一致，否则抛异常）</summary>
        public void Set<T>(string key, T value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            _map ??= new();
            if (_map.TryGetValue(key, out var existing))
            {
                if (existing is not Bucket<T> slot)
                    throw new InvalidOperationException($"Key '{key}' already exists with type {existing.ValueType.FullName}, cannot set as {typeof(T).FullName}.");
                slot.Value = value;
                return;
            }

            _map[key] = new Bucket<T>(value);
        }

        /// <summary>读取（类型必须匹配，否则抛异常；不存在抛 KeyNotFoundException）</summary>
        public T Get<T>(string key)
        {
            while (true)
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
                _map ??= new();
                if (!_map.TryGetValue(key, out var existing))
                {
                    Set(key, (T)default);
                    continue;
                }

                if (existing is Bucket<T> slot) return slot.Value;

                throw new InvalidOperationException($"Key '{key}' is of type {existing.ValueType.FullName}, cannot get as {typeof(T).FullName}.");
            }
        }

        /// <summary>尝试读取（不存在或类型不匹配时返回 false）</summary>
        public bool TryGet<T>(string key, out T value)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            if (_map.TryGetValue(key, out var existing) && existing is Bucket<T> slot)
            {
                value = slot.Value;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>检查某个 key 是否存在</summary>
        public bool ContainsKey(string key)
        {
            return key is null ? 
                throw new ArgumentNullException(nameof(key)) : 
                _map.ContainsKey(key);
        }

        /// <summary>移除某个 key</summary>
        public bool Remove(string key)
        {
            return key is null ? 
                throw new ArgumentNullException(nameof(key)) : 
                _map.Remove(key);
        }

        /// <summary>清空所有项</summary>
        public void Clear()
        {
            _map ??= new();
            _map.Clear();
        }

        /// <summary>
        /// 将所有项的值重置为各自类型的 default(T)。
        /// 例如 int->0, bool->false, 引用类型->null, struct->default(struct)。
        /// </summary>
        public void ResetDefault()
        {
            if (_map == null) return;
            foreach (var kv in _map) kv.Value.ResetToDefault();
        }

        /// <summary>可选：获取某个 key 当前存储的真实类型</summary>
        public Type GetStoredType(string key)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));

            return !_map.TryGetValue(key, out var existing) ? 
                throw new KeyNotFoundException($"Key '{key}' not found.") : 
                existing.ValueType;
        }

        // --- 内部类型：每个槽位保存强类型字段，值类型不会装箱 ---
        private interface IBucket
        {
            Type ValueType { get; }
            void ResetToDefault();
        }

        private sealed class Bucket<T> : IBucket
        {
            public T Value;
            public Bucket(T value) => Value = value;
            public Type ValueType => typeof(T);
            public void ResetToDefault() => Value = default!;
        }
    }
}
