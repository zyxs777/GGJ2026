#nullable enable
using System;
using System.Collections.Generic;

namespace STool.CollectionUtility
{
    public delegate void ValueChanged<in T>(T value);

    public sealed class DecoratedValue<T>
    {
        private struct Entry
        {
            public int Id;
            public T Arg;
            public Func<T, T, T> Apply;

            public Entry(int id, T arg, Func<T, T, T> apply)
            {
                Id = id;
                Arg = arg;
                Apply = apply ?? throw new ArgumentNullException(nameof(apply));
            }
        }

        public readonly struct ModifierCollectionToken : IDisposable, IEquatable<ModifierCollectionToken>
        {
            private readonly DecoratedValue<T>? _owner;
            public readonly int Id;

            internal ModifierCollectionToken(DecoratedValue<T> owner, int id)
            {
                _owner = owner;
                Id = id;
            }

            /// <summary>释放该 token 对应的修饰项（若仍存在）。</summary>
            public void Dispose() => _owner?.Remove(this);

            /// <summary>修改修饰值 Arg。recompute=true 时立即重算并回调。</summary>
            public bool SetValue(T newArg, bool recompute = true)
                => _owner != null && _owner.TrySetArg(this, newArg, recompute);

            /// <summary>读取修饰值 Arg。</summary>
            public bool TryGetValue(out T arg)
            {
                if (_owner == null) { arg = default!; return false; }
                return _owner.TryGetArg(this, out arg);
            }

            /// <summary>修改修饰方法 Apply。recompute=true 时立即重算并回调。</summary>
            public bool SetApply(Func<T, T, T> newApply, bool recompute = true)
            {
                if (newApply == null) throw new ArgumentNullException(nameof(newApply));
                return _owner != null && _owner.TrySetApply(this, newApply, recompute);
            }

            /// <summary>读取修饰方法 Apply。</summary>
            public bool TryGetApply(out Func<T, T, T> apply)
            {
                if (_owner == null) { apply = default!; return false; }
                return _owner.TryGetApply(this, out apply);
            }

            /// <summary>
            /// 批量 SetValue/SetApply(recompute:false) 后，用 token 触发一次重算。
            /// </summary>
            public void Recompute() => _owner?.Recompute();

            public bool Equals(ModifierCollectionToken other)
                => Id == other.Id && ReferenceEquals(_owner, other._owner);

            public override bool Equals(object? obj) => obj is ModifierCollectionToken t && Equals(t);
            public override int GetHashCode() => HashCode.Combine(_owner, Id);
            public static bool operator ==(ModifierCollectionToken left, ModifierCollectionToken right) => left.Equals(right);
            public static bool operator !=(ModifierCollectionToken left, ModifierCollectionToken right) => !left.Equals(right);
            public override string ToString() => $"Token(Id={Id})";
        }

        [NonSerialized] private readonly List<Entry> _entries = new();
        [NonSerialized] private readonly Dictionary<int, int> _indexById = new(); // Id -> index
        private int _nextId = 1;

        private T _baseValue;

        public ValueChanged<T>? OnValueChanged { get; set; }

        public DecoratedValue(T baseValue, ValueChanged<T>? onValueChanged = null)
        {
            _baseValue = baseValue;
            Value = baseValue;
            OnValueChanged = onValueChanged;
        }

        public T BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                RecomputeAndNotify();
            }
        }

        public T Value { get; private set; }

        public ModifierCollectionToken Add(T arg, Func<T, T, T> apply)
        {
            var id = _nextId++;
            var idx = _entries.Count;

            _entries.Add(new Entry(id, arg, apply));
            _indexById[id] = idx;

            RecomputeAndNotify();
            return new ModifierCollectionToken(this, id);
        }

        public bool Remove(ModifierCollectionToken token)
        {
            if (!TryGetIndex(token.Id, out var idx))
                return false;

            RemoveAtIndex(idx);
            RecomputeAndNotify();
            return true;
        }

        public void Clear()
        {
            if (_entries.Count == 0) return;
            _entries.Clear();
            _indexById.Clear();
            RecomputeAndNotify();
        }

        /// <summary>手动触发一次重算（批量更新用）。</summary>
        public void Recompute() => RecomputeAndNotify();

        // ===== Token 调用的内部 API（对称：Set/TryGet） =====

        internal bool TrySetArg(ModifierCollectionToken token, T newArg, bool recompute)
        {
            if (!TryGetIndex(token.Id, out var idx))
                return false;

            var e = _entries[idx];
            e.Arg = newArg;
            _entries[idx] = e;

            if (recompute) RecomputeAndNotify();
            return true;
        }

        internal bool TryGetArg(ModifierCollectionToken token, out T arg)
        {
            if (!TryGetIndex(token.Id, out var idx))
            {
                arg = default!;
                return false;
            }

            arg = _entries[idx].Arg;
            return true;
        }

        internal bool TrySetApply(ModifierCollectionToken token, Func<T, T, T> newApply, bool recompute)
        {
            if (newApply == null) throw new ArgumentNullException(nameof(newApply));
            if (!TryGetIndex(token.Id, out var idx))
                return false;

            var e = _entries[idx];
            e.Apply = newApply;
            _entries[idx] = e;

            if (recompute) RecomputeAndNotify();
            return true;
        }

        internal bool TryGetApply(ModifierCollectionToken token, out Func<T, T, T> apply)
        {
            if (!TryGetIndex(token.Id, out var idx))
            {
                apply = default!;
                return false;
            }

            apply = _entries[idx].Apply;
            return true;
        }

        private bool TryGetIndex(int id, out int idx) => _indexById.TryGetValue(id, out idx);

        private void RemoveAtIndex(int idx)
        {
            var removedId = _entries[idx].Id;
            _indexById.Remove(removedId);

            var lastIdx = _entries.Count - 1;
            if (idx != lastIdx)
            {
                // swap-remove
                var last = _entries[lastIdx];
                _entries[idx] = last;
                _indexById[last.Id] = idx;
            }

            _entries.RemoveAt(lastIdx);
        }

        private void RecomputeAndNotify()
        {
            var result = _baseValue;

            for (var i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                result = e.Apply(result, e.Arg);
            }

            Value = result;
            OnValueChanged?.Invoke(Value);
        }
    }
}
