using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SAddress;
using STool.SInterfaces;
using UnityEngine;

namespace STool.SSequencer.Adapter
{
    public sealed class SAddrSequencerRouter : SerializedMonoBehaviour, 
        ISAddressableRouter,
        ISAddressable,
        IBakeTarget
    { 
        public ulong Address { get => Router.Address; set=> Router.Address = value; }
        public string Guid { get => Router.Guid; set => Router.Guid = value; }

        public ISAddressable GetAddressable() => this;
        [HideReferenceObjectPicker] [NonSerialized] [OdinSerialize]
        public SAddrRouter Router = new();


        public void Resolve(ISAddress add)
        { 
            Router.Resolve(add);
        }
        public List<ValueDropdownItem> GetServices()
        {
            var list = new List<ValueDropdownItem>();
            for (var index = Router.Addressable.Count - 1; index >= 0; index--)
            {
                //Remove invalid router/IAddr
                var addressable = Router.Addressable[index];
                if (addressable == null || (addressable is UnityEngine.Object obj && obj == null))
                {
                    Router.Addressable.RemoveAt(index);
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
                    continue;                    
                    #endif
                }
                
                var childList = addressable.GetServices();
                foreach (var child in childList)
                    if (child.Value is ISAddress isa)
                    {
                        isa.Address = SAddrBitUtility.MergeAddr(isa.Address, Address);
                        list.Add(new ValueDropdownItem($"{name}/{child.Text}", child.Value));
                    }
            }

            return list;
        }


#if UNITY_EDITOR
        public void DoBake() => Organize();
        [Button] public void Organize()
        {
            if (Application.isPlaying) return;  //运行时禁用
            
            BakeChild(transform);
            DoOnTransform(transform);
            Router.Addressable.Remove(this);
            Router.DoDistribute();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        private void DoOnTransform(Transform target)
        {
            //遍历target下所有child
            var childCnt = target.childCount;
            for (var i = 0; i < childCnt; i++)
            {
                var child = target.GetChild(i);
                //如果child身上有Router，则在该层停止并收纳此router，调用该router组网 //否则递归遍历
                if (child.TryGetComponent(out ISAddressableRouter sar))
                {
                    var isa = sar.GetAddressable();
                    if (!Router.Addressable.Contains(isa)) Router.Addressable.Add(isa);
                    sar.Organize();
                }
                else
                {
                    BakeChild(child);
                    DoOnTransform(child);
                }
            }
        }
        private void BakeChild(Transform target)
        {
            var comps = target.GetComponents<ISAddressable>();
            foreach (var comp in comps)
            {
                if (Router.Addressable.Contains(comp)) continue;
                Router.Addressable.Add(comp);
            }
        }

        private readonly Dictionary<string, string> _guidMapping = new();
        [FoldoutGroup("Tool")][HorizontalGroup("Tool/1")] [SerializeField] private bool guidReGenLock = true;
        [FoldoutGroup("Tool")] [HorizontalGroup("Tool/1")]
        [Button] private void ReGenerateGuidForChildren()
        {
            if (guidReGenLock)
            {
                Debug.LogWarning($"Please Unlock to Fix");
                return;
            }
            if (Application.isPlaying) return;
            
            //Guid 重置
            _guidMapping.Clear();
            var comps = transform.GetComponentsInChildren<ISAddressable>();
            foreach (var comp in comps)
            {
                var oldGuid = comp.Guid;
                if (string.IsNullOrEmpty(oldGuid)) continue;
                
                var newGuid = System.Guid.NewGuid().ToString();
                comp.Guid = newGuid;
                _guidMapping.Add(oldGuid, newGuid);
            }
            guidReGenLock = true;
            
            //相关质量的Guid修复
            var fixes = transform.GetComponentsInChildren<ISAddressGuidFix>();
            foreach (var fix in fixes)
            {
                fix.FixAddress(_guidMapping);
            }
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
        #endif
    }
}
