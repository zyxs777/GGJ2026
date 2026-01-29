namespace STool
{
    /// <summary>
    /// 抽象懒汉式单例基类
    /// 使用时继承此类，保证子类有一个受保护的无参构造函数
    /// </summary>
    /// <typeparam name="T">单例类</typeparam>
    public abstract class LazySingleton<T> where T : class
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock) // 确保多线程安全
                {
                    _instance ??= (T)System.Activator.CreateInstance(typeof(T), true // 允许调用非public构造函数
                    );
                }
                return _instance;
            }
        }

        /// <summary>
        /// 子类必须提供一个受保护的无参构造函数
        /// </summary>
        protected LazySingleton() { }
    }
}