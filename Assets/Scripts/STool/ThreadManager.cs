using System;
using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;

namespace STool
{
    public sealed class ThreadManager
    {
        #region Singleton
        private static ThreadManager _instance;
        public static ThreadManager Instance => _instance ??= new ThreadManager();
        private ThreadManager()
        {
            UnityEngine.Debug.Log($"Thread Manager initialized {this.GetHashCode()}");
        }
        #endregion
        #region Thread Management
        private readonly Dictionary<int, ThreadSlot> _threads = new();
        private sealed class ThreadSlot
        {
            private readonly Thread _t;
            private Action _act;
            private volatile bool _running = true;
            public bool Debug;
            public ThreadSlot()
            {
                _t = new Thread(UpdaterWithDebbuger);
                Reset();
                // Debug.Log($"Thread started {this.GetHashCode()} with {_t.GetHashCode()} c {_act.GetHashCode()} ");
            }
            public void Stop()
            {
                _running = false;
                _act = null;
            }
            public void Abort()
            {
                Stop();
                _t.Abort();
            }
            public void Reset()
            {
                _act = DoNothing;
                _t.IsBackground = true;
                if(!_t.IsAlive) _t.Start();
            }
            private static void DoNothing(){}
            private void UpdaterWithDebbuger()
            {
                while (_running)
                {
                    try
                    {
                        _act.Invoke();
                    }
                    catch (Exception e)
                    {
                        if(Debug) Console.WriteLine(e);
                        throw;
                    } 
                }
            }
            public void AddTask(Action act)
            {
                _act += act;
                // Debug.Log($"Method added {act.GetHashCode()} => {this._act.GetHashCode()}");
            }
            public void RemoveTask(Action act){this._act -= act;}
        }
        
        public void Shutdown()
        {
            foreach (var variable in _threads)
            {
                var t = variable.Value;
                t.Stop();
            }
        }
        public void ForceShutdown()
        {
            foreach (var variable in _threads)
            {
                var t = variable.Value;
                t.Abort();
            }
            _threads.Clear();
        }
        public void Init()
        {
            foreach (var variable in _threads)
            {
                variable.Value.Reset();
            }
        }
        public void AddLongTask(int threadId,Action action)
        {
            if (!_threads.TryGetValue(threadId, out var threadSlot))
            {
                threadSlot = new ThreadSlot();
                _threads.Add(threadId, threadSlot);
            }
            threadSlot.AddTask(action);
        }
        public void RemoveLongTask(int threadId, Action action)
        {
            if (!_threads.TryGetValue(threadId, out var threadSlot)) return;
            threadSlot.RemoveTask(action);
        }
        #endregion
        #region Thread Collections Management

        

        #endregion
    }
    
    [System.Serializable] public class ThreadSafeCounter
    {
        [ShowInInspector]
        private int _value;
        public ThreadSafeCounter(int initialValue = 0)
        {
            _value = initialValue;
        }
        // 增加并返回增加后的值
        public int Increment()
        {
            return Interlocked.Increment(ref _value);
        }
        // 减少并返回减少后的值
        public int Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }
        // 尝试减少1，如果为0则返回false，不执行
        public bool TryConsume()
        {
            var current = _value;
            if (current <= 0) return false;
            return Interlocked.CompareExchange(ref _value, current - 1, current) == current;
        }
        // 读取当前值
        public int GetValue()
        {
            return Interlocked.Add(ref _value, 0);
        }
        // 清空为0
        public void Reset()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }
}
