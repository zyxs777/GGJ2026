using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace STool.SAddress
{
    [Serializable] public sealed class SAddrRouter : ISAddressable
    {
        [ValueDropdown("GetSegments")] [HideLabel]
        public SAddrSegment segment;
        
        #region Segment Provider
        #if UNITY_EDITOR
        private List<ValueDropdownItem> GetSegments()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SAddressConfig");
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            var assets = UnityEditor.AssetDatabase.LoadAssetAtPath<SAddressConfig>(path);
            return !assets ? null : assets.SegmentsProvider();
        }

        public void DoDistribute()
        {
            for (var i = Addressable.Count - 1; i >= 0; i--)
                if (Addressable[i] == null || (Addressable[i] is UnityEngine.Object obj && obj == null))
                    Addressable.RemoveAt(i);
            SAddrBitUtility.AllocateNewAddr(Addressable, segment.offset, segment.width);
        }
        #endif
        #endregion
        [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][OdinSerialize] 
        private ulong _address;
        [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][OdinSerialize] 
        private string _guid = System.Guid.NewGuid().ToString();
        public ulong Address { get =>_address; set=>_address = value; }
        public string Guid { get => _guid; set => _guid = value; }

        [OdinSerialize] public List<ISAddressable> Addressable = new();

        public void Resolve(ISAddress add)
        {
            for (var i = Addressable.Count - 1; i >= 0; i--)
            {
                var isa = Addressable[i];
                if (!SAddrBitUtility.MatchByMask(add.Address, isa.Address, segment.GetMask())) continue;
                isa.Resolve(add);
            }
        }
        public List<ValueDropdownItem> GetServices()
        {
            var list = new List<ValueDropdownItem>();
            for (var i = Addressable.Count - 1; i >= 0; i--)
            {
                var iAddr = Addressable[i];
                if (iAddr == null) continue;
                list.AddRange(iAddr.GetServices());
            }
            return list;
        }
    }
}
