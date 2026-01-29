using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace STool.SAddress
{
    /// <summary>
    /// 表示本对象上存有地址，可用于寻址和转发
    /// </summary>
    public interface ISAddress
    {
        public ulong Address { get; set; }
        public string Guid { get; set; }
    }

    /// <summary>
    /// 表示这是可寻址对象，持有地址，会接收传入内容并解析
    /// </summary>
    public interface ISAddressable
    {
        public ulong Address { get; set; }
        public string Guid { get; set; }
        public void Resolve(ISAddress add);
        public List<ValueDropdownItem> GetServices();
    }

    /// <summary>
    /// 可寻址转发路由
    /// </summary>
    public interface ISAddressableRouter
    {
        /// <summary>
        /// 本网关
        /// </summary>
        /// <returns></returns>
        public ISAddressable GetAddressable();
        
        #if UNITY_EDITOR
        /// <summary>
        /// 组网
        /// </summary>
        public void Organize();
        #endif
    }

    //Guid 修复入口（针对旧Guid转新Guid）
    public interface ISAddressGuidFix
    {
        #if UNITY_EDITOR
        public void FixAddress(Dictionary<string, string> mapping);
        #endif
    }
}
