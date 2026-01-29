using System;
using System.Collections.Generic;

namespace STool.STypeEventBus
{
    /// <summary>
    /// Management for Disposable Targets, once registered, dispose in one go. 
    /// Not Designed for Mult-Thread yet
    /// </summary>
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly Dictionary<Type, List<IDisposable>> _subscriptions = new();
        
        public void Add<T>(IDisposable disposable)
        {
            var key = typeof(T);
            if (_subscriptions.TryGetValue(key, out var old))
            {
                old.Add(disposable);
            }
            else
            {
                _subscriptions[key] = new List<IDisposable> { disposable };
            }
        }

        public void Remove<T>()
        {
            if (!_subscriptions.TryGetValue(typeof(T), out var disposables)) return;
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            _subscriptions.Remove(typeof(T));
        }
        
        public void Dispose()
        {
            foreach (var disposables in _subscriptions.Values)
            foreach (var disposable in disposables)
                disposable.Dispose();
            _subscriptions.Clear();
        }
    }
}
