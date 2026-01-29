using System;
using System.Collections.Generic;
using UnityEngine;

namespace STool
{
    public sealed class ThreadLocalContainerPool
    {
        private static ThreadLocalContainerPool _instance;
        public static ThreadLocalContainerPool Instance => _instance ??= new ThreadLocalContainerPool();

        private ThreadLocalContainerPool()
        {
        }

        private readonly Dictionary<int, Dictionary<Type, object>> _threadPools = new();

        // === Debug 控制项 ===
        public bool enableDebugLog = false;
        public int maxRetainPerType = 64;

        private Dictionary<Type, object> GetOrCreatePool(int threadId)
        {
            if (_threadPools.TryGetValue(threadId, out var pool)) return pool;
            pool = new Dictionary<Type, object>();
            _threadPools[threadId] = pool;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            if (enableDebugLog) Debug.Log("[ThreadLocalPool] [Init] Thread " + threadId + " pool created");
            return pool;
        }

        public List<T> RentList<T>(int threadId)
        {
            var pool = GetOrCreatePool(threadId);
            var type = typeof(List<T>);

            if (!pool.TryGetValue(type, out var stackObj))
            {
                stackObj = new Stack<List<T>>();
                pool[type] = stackObj;
            }

            var stack = (Stack<List<T>>)stackObj;

            List<T> list;
            if (stack.Count > 0)
            {
                list = stack.Pop();
                list.Clear();

                if (enableDebugLog)
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    Debug.Log("[ThreadLocalPool] [Rent] Thread " + threadId + " <" + type.Name +
                              "> reused, remaining: " + stack.Count);
            }
            else
            {
                list = new List<T>();

                if (enableDebugLog)
                    Debug.Log("[ThreadLocalPool] [Rent] Thread " + threadId + " <" + type.Name + "> created new");
            }

            return list;
        }
        public void ReturnList<T>(int threadId, List<T> list)
        {
            var pool = GetOrCreatePool(threadId);
            var type = typeof(List<T>);

            if (!pool.TryGetValue(type, out var stackObj))
            {
                stackObj = new Stack<List<T>>();
                pool[type] = stackObj;
            }

            var stack = (Stack<List<T>>)stackObj;
            if (stack.Count < maxRetainPerType)
            {
                stack.Push(list);

                if (enableDebugLog)
                    Debug.Log("[ThreadLocalPool] [Return] Thread " + threadId + " <" + type.Name +
                              "> returned, total: " + stack.Count);
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log("[ThreadLocalPool] [Drop] Thread " + threadId + " <" + type.Name +
                              "> dropped (maxRetain=" + maxRetainPerType + ")");
            }
        }
        public void ClearThread(int threadId)
        {
            if (!_threadPools.Remove(threadId)) return;
            if (enableDebugLog) Debug.Log("[ThreadLocalPool] [Clear] Thread " + threadId + " container pool cleared");
        }
        public void ClearAll()
        {
            _threadPools.Clear();
            if (enableDebugLog) Debug.Log("[ThreadLocalPool] [Clear] All thread container pools cleared");
        }
        public Dictionary<string, int> GetThreadPoolState(int threadId)
        {
            var result = new Dictionary<string, int>();
            if (_threadPools.TryGetValue(threadId, out var pool))
            {
                foreach (var kv in pool)
                {
                    var count = kv.Value switch
                    {
                        System.Collections.ICollection col => col.Count,
                        _ => 0
                    };
                    result[kv.Key.Name] = count;
                }
            }

            return result;
        }
        public void DumpAllPoolStates()
        {
            foreach (var thread in _threadPools)
            {
                Debug.Log("[ThreadLocalPool] [Thread " + thread.Key + "]");
                foreach (var kv in thread.Value)
                {
                    var count = kv.Value switch
                    {
                        System.Collections.ICollection col => col.Count,
                        _ => 0
                    };
                    Debug.Log("    - " + kv.Key.Name + ": " + count);
                }
            }
        }
    }
    public static class ThreadLocalCode
    {
        /// <summary>
        /// 主线程默认代号
        /// </summary>
        public static int MainThreadId = -1;
        /// <summary>
        /// 轻量物理更新、小型敌人更新
        /// </summary>
        public static int LightPhysics = 0;
    }
}

namespace System.Collections.Concurrent
{
    public class ConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dict = new();
        public bool Add(T item) => _dict.TryAdd(item, 0);
        public bool Remove(T item) => _dict.TryRemove(item, out _);
        public bool Contains(T item) => _dict.ContainsKey(item);
        public void Clear() => _dict.Clear();
        public int Count => _dict.Count;
    }
}