#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace STool.SBlackBoard
{
    public interface IBlackboard
    {
        IBlackboard? Parent { get; }

        bool TryGet<T>(BBKey<T> key, out T value);
        T GetOrDefault<T>(BBKey<T> key);
        bool Contains<T>(BBKey<T> key);

        void Set<T>(BBKey<T> key, T value);
        void Create<T>(BBKey<T> key, T value);
        bool Remove<T>(BBKey<T> key);

        int Version { get; }

        IDisposable Subscribe(BBKey key, Action<BBKey> onChanged);
    }

    public sealed class Blackboard : IBlackboard
    {
        // 每个 T 一个 Store<T>，避免 Dictionary<BBKey, object> 带来的装箱
        private readonly Dictionary<Type, object> _stores = new();
 
        // 订阅仍然按“Key”维度：谁变了通知谁（不依赖值存储方式）
        private readonly Dictionary<BBKey, List<Action<BBKey>>> _watchers = new();

        public IBlackboard? Parent { get; set; }
        public int Version { get; private set; }

        public Blackboard(IBlackboard? parent = null) => Parent = parent;

        private Store<T> GetStore<T>()
        {
            var t = typeof(T);
            if (_stores.TryGetValue(t, out var obj)) return (Store<T>)obj;

            var store = new Store<T>();
            _stores[t] = store;
            return store;
        }

        public bool TryGet<T>(BBKey<T> key, out T value)
        {
            var store = GetStore<T>();
            if (store.Data.TryGetValue(key, out value)) return true;

            if (Parent != null && Parent.TryGet(key, out value)) return true;

            value = default!;
            return false;
        }
        public T GetOrDefault<T>(BBKey<T> key) => TryGet(key, out var v) ? v : key.DefaultValue;
        public bool Contains<T>(BBKey<T> key)
        {
            var store = GetStore<T>();
            return store.Data.ContainsKey(key) || (Parent?.Contains(key) ?? false);
        }
        public void Set<T>(BBKey<T> key, T value)
        {
            var store = GetStore<T>();
            if (!store.Data.ContainsKey(key))
                Parent?.Set(key, value);
            else
                store.Data[key] = value;

            Version++;
            Notify(key);
        }

        public void Create<T>(BBKey<T> key, T value)
        {
            var store = GetStore<T>();
            store.Data[key] = value;
        }

        public bool Remove<T>(BBKey<T> key)
        {
            var store = GetStore<T>();
            if (!store.Data.Remove(key))
                return false;

            Version++;
            Notify(key);
            return true;
        }
        public IDisposable Subscribe(BBKey key, Action<BBKey> onChanged)
        {
            if (!_watchers.TryGetValue(key, out var list))
            {
                list = new List<Action<BBKey>>();
                _watchers[key] = list;
            }

            list.Add(onChanged);
            return new Subscription(this, key, onChanged);
        }
        private void UnsubscribeInternal(BBKey key, Action<BBKey> onChanged)
        {
            if (!_watchers.TryGetValue(key, out var list)) return;
            list.Remove(onChanged);
            if (list.Count == 0) _watchers.Remove(key);
        }
        private void Notify(BBKey key)
        {
            if (!_watchers.TryGetValue(key, out var list)) return;

            // 快照避免回调里订阅/退订导致遍历异常
            for (var i = list.Count - 1; i >= 0; i--)
                list[i]?.Invoke(key);
        }
        private sealed class Store<T>
        {
            public readonly Dictionary<BBKey<T>, T> Data = new();
            public override string ToString()
            {
                var str = $"[{typeof(T)}]";
                foreach (var val in Data)
                {
                    str += $"\n{val.Key.Name}\t{val.Value}";
                }
                return str;
            }
        }
        private sealed class Subscription : IDisposable
        {
            private Blackboard? _bb;
            private BBKey? _key;
            private Action<BBKey>? _cb;

            public Subscription(Blackboard bb, BBKey key, Action<BBKey> cb)
            {
                _bb = bb;
                _key = key;
                _cb = cb;
            }

            public void Dispose()
            {
                if (_bb == null || _key == null || _cb == null) return;
                _bb.UnsubscribeInternal(_key, _cb);
                _bb = null;
                _key = null;
                _cb = null;
            }
        }

        public override string ToString()
        {
            var str = $"[Blackboard]";
            foreach (var (key, value) in _stores)
            {
                str += $"\n{value}";
            }
            return str;
        }
    }
}