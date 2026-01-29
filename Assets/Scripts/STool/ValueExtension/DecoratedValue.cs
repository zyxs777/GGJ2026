using System;
using System.Collections.Generic;

namespace STool
{
    public sealed class DecoratedValue<T>
    {
        private T _base;                          // 原始值
        private T _decorated;                     // 装饰后的当前值
        private readonly List<Entry> _decorators; // 有序存储装饰器
        private readonly IEqualityComparer<T> _eq;

        private struct Entry
        {
            public int Order;
            public Func<T, T> Func;
        }

        public DecoratedValue(T initialValue, IEqualityComparer<T> comparer = null)
        {
            _base = initialValue;
            _decorated = initialValue;
            _decorators = new List<Entry>(4);
            _eq = comparer ?? EqualityComparer<T>.Default;
        }

        /// <summary>原始值（可读可写）。设置时会自动刷新。</summary>
        public T Base
        {
            get => _base;
            set
            {
                if (_eq.Equals(_base, value)) return;
                _base = value;
                Refresh();
            }
        }

        /// <summary>当前装饰后的值（只读）。</summary>
        public T Value => _decorated;

        /// <summary>装饰后值变更事件（oldValue, newValue）。</summary>
        public event Action<T, T> OnValueChanged;

        /// <summary>
        /// 新增：单参数泛型委托（事件），当装饰器刷新后广播当前“装饰后的值”。
        /// 典型订阅：dv.OnUpdated += v => { ... };
        /// </summary>
        public event Action<T> OnUpdated;

        /// <summary>
        /// 便捷订阅器：返回 IDisposable 句柄用于移除；可选立即回放当前值。
        /// </summary>
        public IDisposable Subscribe(Action<T> handler, bool emitImmediately = false)
        {
            OnUpdated += handler ?? throw new ArgumentNullException(nameof(handler));
            if (emitImmediately)
            {
                // 立即回放一次当前装饰值
                handler(_decorated);
            }
            return new EventUnsubscriber(this, handler);
        }

        /// <summary>
        /// 注册装饰器。order 越小优先执行。返回 IDisposable 句柄用于移除该装饰器。
        /// 注册完成后会立即刷新。
        /// </summary>
        public IDisposable AddDecorator(Func<T, T> decorator, int order = 0)
        {
            if (decorator == null) throw new ArgumentNullException(nameof(decorator));

            int idx = _decorators.FindIndex(e => order < e.Order);
            var entry = new Entry { Order = order, Func = decorator };
            if (idx >= 0) _decorators.Insert(idx, entry);
            else _decorators.Add(entry);

            Refresh();
            return new DecoratorRemover(this, decorator, order);
        }
        /// <summary>移除与 (decorator, order) 匹配的第一个装饰器。移除成功会刷新。</summary>
        public bool RemoveDecorator(Func<T, T> decorator, int order = 0)
        {
            for (int i = 0; i < _decorators.Count; i++)
            {
                var e = _decorators[i];
                if (e.Func == decorator && e.Order == order)
                {
                    _decorators.RemoveAt(i);
                    Refresh();
                    return true;
                }
            }
            return false;
        }
        /// <summary>清空所有装饰器并刷新。</summary>
        public void ClearDecorators()
        {
            if (_decorators.Count == 0) return;
            _decorators.Clear();
            Refresh();
        }
        /// <summary>手动刷新：把 Base 依次经过所有装饰器，得到 Value。</summary>
        public void Refresh()
        {
            var old = _decorated;
            var cur = _base;

            for (var i = 0; i < _decorators.Count; i++)
                cur = _decorators[i].Func(cur);

            _decorated = cur;

            // 只有真正变化才触发双参数变更事件
            if (!_eq.Equals(old, _decorated))
                OnValueChanged?.Invoke(old, _decorated);

            // 单参数事件：每次 Refresh 都广播当前值（符合“装饰器发生更新即广播”的需求）
            OnUpdated?.Invoke(_decorated);
        }

        /// <summary>便捷：直接设置原始值并获得装饰结果。</summary>
        public T SetAndGet(T newBase)
        {
            Base = newBase;
            return _decorated;
        }

        // —— 内部：移除装饰器的句柄 —— //
        private sealed class DecoratorRemover : IDisposable
        {
            private DecoratedValue<T> _owner;
            private readonly Func<T, T> _func;
            private readonly int _order;

            public DecoratorRemover(DecoratedValue<T> owner, Func<T, T> func, int order)
            {
                _owner = owner;
                _func = func;
                _order = order;
            }

            public void Dispose()
            {
                var o = _owner;
                if (o == null) return;
                _owner = null;
                o.RemoveDecorator(_func, _order);
            }
        }

        // —— 内部：移除订阅的句柄 —— //
        private sealed class EventUnsubscriber : IDisposable
        {
            private DecoratedValue<T> _owner;
            private readonly Action<T> _handler;

            public EventUnsubscriber(DecoratedValue<T> owner, Action<T> handler)
            {
                _owner = owner;
                _handler = handler;
            }

            public void Dispose()
            {
                var o = _owner;
                if (o == null) return;
                _owner = null;
                o.OnUpdated -= _handler;
            }
        }
    }
}
