using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.CollectionUtility
{
    /// <summary>
    /// 带自复用管理的容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ReusableCollection<T> where T : class
    {
        public ReusableCollection(Func<T> createMethod, Action<T> destroyMethod, Action<T> getMethod = null, Action<T> hideMethod = null)
        {
            this.createMethod = createMethod;
            this.destroyMethod = destroyMethod;
            this.getMethod = getMethod;
            this.hideMethod = hideMethod;
        }

        private Func<T> createMethod;
        private Action<T> destroyMethod;
        private Action<T> getMethod;
        private Action<T> hideMethod;
        
        [ReadOnly, ShowInInspector] private Queue<T> waitList = new();
        [ReadOnly, ShowInInspector] private List<T> onUseList = new();
        public IReadOnlyList<T> OnUseList => onUseList;
        
        public T Get()
        {
            if (!waitList.TryDequeue(out var item))
            {
                item = createMethod();
            }
            onUseList.Add(item);
            getMethod?.Invoke(item);
            return item;
        }
        public bool Push(T item)
        {
            if (!onUseList.Contains(item)) return false;
            hideMethod?.Invoke(item);
            onUseList.Remove(item);
            waitList.Enqueue(item);
            return true;
        }
        public void ResetAll()
        {
            foreach (var item in onUseList)
                destroyMethod(item);
            foreach (var item in waitList)
                destroyMethod(item);
            waitList.Clear();
            onUseList.Clear();
        }
        /// <summary>
        /// 将所有活跃对象清理回池中
        /// </summary>
        public void ClearToPool()
        {
            foreach (var item in onUseList)
            {
                hideMethod?.Invoke(item);
                waitList.Enqueue(item);
            }
            onUseList.Clear();
        }
        /// <summary>
        /// 按指定条件将活跃对象推入池
        /// </summary>
        /// <param name="selector"></param>
        public void ConditionalPush(Func<T, bool> selector)
        {
            for (var index = onUseList.Count - 1; index >= 0; index--)
            {
                var item = onUseList[index];
                if (!selector(item)) continue;
                hideMethod?.Invoke(item);
                waitList.Enqueue(item);
                onUseList.RemoveAt(index);
            }
        }
        /// <summary>
        /// 把指定队列的对象推入池中（如果是本容器所有的）
        /// </summary>
        /// <param name="collection"></param>
        public void PushRange(List<T> collection)
        {
            foreach (var item in collection)
                Push(item);
        }
    }
}
