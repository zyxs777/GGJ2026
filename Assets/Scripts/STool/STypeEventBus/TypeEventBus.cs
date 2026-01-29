using System;
using System.Collections.Generic;

namespace STool.STypeEventBus
{
    /// <summary>
    /// 基于类的事件管线
    /// </summary>
    public class TypeEventBus : ITypeBusSubscriber, ITypeBusPublisher
    {
        private readonly Dictionary<Type, Delegate> _eventTable = new();
        public void Reset()
        {
            _eventTable.Clear();
        }
        
        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_eventTable.TryGetValue(type, out var existing))
                _eventTable[type] = (Action<T>)existing + handler;
            else
                _eventTable[type] = handler;

            // 关键：返回一个 token，Dispose 时自动退订
            return new SubscriptionToken<T>(this, handler);
        }
        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_eventTable.TryGetValue(type, out var existing)) return;
            var current = (Action<T>)existing - handler;
            if (current == null)
                _eventTable.Remove(type);
            else
                _eventTable[type] = current;
        }
        public void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!_eventTable.TryGetValue(type, out var del)) return;
            var callback = (Action<T>)del;
            callback?.Invoke(evt);
        }
        
        
        private sealed class SubscriptionToken<T> : IDisposable
        {
            private TypeEventBus _bus;
            private Action<T> _handler;
            public SubscriptionToken(TypeEventBus bus, Action<T> handler)
            {
                _bus = bus;
                _handler = handler;
            }
            public void Dispose()
            {
                // 防止重复 Dispose
                if (_bus == null || _handler == null) return;

                _bus.Unsubscribe(_handler);
                _bus = null;
                _handler = null;
            }
        }
        public override string ToString()
        {
            var str = $"[TypeEventBus] {GetHashCode()}";
            foreach (var func in _eventTable)
            {
                str += $"\n{func.Key.Name}\t:{func.Value}";
            }
            return str;
        }
    }

    /// <summary>
    /// 基于类的查询管线
    /// </summary>
    public class TypeFuncBus
    {
        private readonly Dictionary<Type, Delegate> _funcTable = new();
        public void Reset()
        {
            _funcTable.Clear();
        }
        public void Register<TV,T>(Func<TV,T> func) 
            where T : IQueryEvent 
            where TV: IQueryParam
        {
            var t = typeof(T);
            _funcTable[t] = func;
        }
        public void Unregister<T>() where T : IQueryEvent
        {
            var t = typeof(T);
            _funcTable.Remove(t);
        }
        public T Query<TV, T>(TV param) 
            where T : IQueryEvent
            where TV : IQueryParam
        {
            var t = typeof(T);
            var func = (Func<TV, T>)_funcTable[t];
            return func(param);
        }

        public override string ToString()
        {
            var str = $"[TypeFuncBus] {GetHashCode()}";
            foreach (var func in _funcTable)
            {
                str += $"\n{func.Key.Name}\t:{func.Value}";
            }
            return str;
        }
    }
}
