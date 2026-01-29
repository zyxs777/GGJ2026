using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace STool.SBlackBoard
{
    /// <summary>非泛型 Key 基类，用于做字典 key 与订阅。</summary>
    public abstract record BBKey(string Name)
    {
        [ShowInInspector] public string Name { get; set; } = Name;
    }

    /// <summary>类型安全 Key。</summary>
    public sealed record BBKey<T>(string Name, T DefaultValue = default!) : BBKey(Name)
    {
        public T DefaultValue { get; } = DefaultValue;
    }
    
    public static class BBKeyShare
    {
        private static readonly Dictionary<Type, BBKey> SharedKey = new();
        
        /// <summary>
        /// 获取一个复用的BBKey，不支持多线程，这个Key仅供一次性使用
        /// </summary>
        /// <param name="keyName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static BBKey<T> GetSharedKey<T>(string keyName)
        {
            var t = typeof(T);
            if (!SharedKey.TryGetValue(t, out _)) SharedKey.Add(t, new BBKey<T>(keyName));
            
            var result = (BBKey<T>)SharedKey[t];
            result.Name = keyName;
            return result;
        }
    }
}
