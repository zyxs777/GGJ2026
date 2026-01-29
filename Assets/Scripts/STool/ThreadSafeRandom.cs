using System;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace STool
{
    /// <summary>
    /// 线程安全的随机数工具类（适用于 Unity 多线程环境）
    /// </summary>
    public static class ThreadSafeRandom
    {
        // 每个线程拥有自己的 Random 实例，确保线程安全
        private static readonly ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() =>
        {
            // 可以基于时间戳 + 线程ID 初始化，避免同种种子
            return new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
        });
        private static Random Rand => threadLocalRandom.Value;
        public static int Range(int minInclusive, int maxExclusive)
        {
            return Rand.Next(minInclusive, maxExclusive);
        }
        public static float Range(float minInclusive, float maxInclusive)
        {
            return (float)(Rand.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
        }
        public static bool Bool()
        {
            return Rand.Next(0, 2) == 0;
        }
        public static float Value01()
        {
            return (float)Rand.NextDouble(); // 0 ~ 1
        }
        public static Vector2 InsideUnitCircle()
        {
            var x = Range(-1f, 1f);
            var z = Range(-1f, 1f);
            var v = new Vector2(x, z);
            return v.normalized;
        }
        public static Vector3 InsideUnitSphere()
        {
                var x = Range(-1f, 1f);
                var y = Range(-1f, 1f);
                var z = Range(-1f, 1f);
                var v = new Vector3(x, y, z);
                return v.normalized;
        }
        public static void Shuffle<T>(T[] array)
        {
            for (var i = array.Length - 1; i > 0; i--)
            {
                int j = Rand.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}
