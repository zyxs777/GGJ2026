using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using STool.CollectionUtility;
using STool.SAddress;
using STool.SConditions;
using STool.SInterfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace STool.SSequencer.Adapter
{
    public sealed class SAddressableSequencer : SerializedMonoBehaviour
        , ISAddressable
        , ISSequenceStepDealer
        , ITimeUpdater
        ,ISAddressGuidFix
    {
        #region Mono Initialize
        private void Awake()
        {
            sSequencer.IValueSource = IValueSource;
            sSequencer.IStepOut = this;
        }
        
        #endregion
        #region Addressable
        //Receive and Handle
        [HorizontalGroup("Addr")] [SerializeField] [ReadOnly] [HideLabel]
        private ulong address;
        [HorizontalGroup("Addr")] [SerializeField] [ReadOnly] [HideLabel]
        private string guid = System.Guid.NewGuid().ToString();
        public ulong Address { get => address; set => address = value; }
        public string Guid { get => guid; set => guid = value; }

        public void Resolve(ISAddress add)
        {
            if (add is not ISSequencePlay isp) return;
            var seqGuid = isp.SeqGuid;
            sSequencer.Play(seqGuid);
        }

        #endregion        
        #region Sequencer
        //Sequencer
        [OnValueChanged("OnSSequencerChange", IncludeChildren = true)]
        public SSequencer sSequencer = new();
        public void TimeUpdate(float deltaTime) => sSequencer.Update(deltaTime);

        //Sequence Value Src
        public IValueSource IValueSource;
        //Sequence output
        public ISAddressable Router;
        public void Handle(ISSequenceStep step)
        {
            if (step is ISAddress iAddr)
            {
                Router.Resolve(iAddr);
            }
        }
        
        #endregion
        #region Editor Support
        public List<ValueDropdownItem> GetServices()
        {
            var list = new List<ValueDropdownItem>();
            foreach (var sequenceSlot in sSequencer.dataSequences)
            {
                var text = $"Seq/{name} Play {sequenceSlot.sequenceEntry.name}";
                var seqName = $"{name}/{sequenceSlot.sequenceEntry.name}";
                list.Add(new ValueDropdownItem()
                {
                    Text = text,
                    Value = new SeqEvt_ExecuteSeq(Address, Guid, sequenceSlot.sequenceEntry.SeqGuid, seqName)
                });
            }
            return list;
        }

        
        #endregion
        #region Mannal Version
        [ValueDropdown("GetManualPlayList")]
        [SerializeReference] private List<string> manualPlayList = new();
        [Button] public void ManualPlay()
        {
            foreach (var manual in manualPlayList)
                Play(manual);
        }
        
        public void Play(string sGuid)
        {
            sSequencer.Play(sGuid);
        }
        #if UNITY_EDITOR
        private List<ValueDropdownItem> GetManualPlayList()
        {
            var list = new List<ValueDropdownItem>();
            foreach (var slot in sSequencer.dataSequences)
            {
                var entry = slot.sequenceEntry;
                list.Add(new ValueDropdownItem()
                {
                    Text = entry.name,
                    Value = entry.SeqGuid
                });
            }
            return list;
        }
        #endif
        #endregion
        #if UNITY_EDITOR
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (Router == null || (Router is Object rObj && !rObj)) FindRouter();
            if (IValueSource == null || (IValueSource is Object iObj && !iObj)) FindValueSource();
            InsertSequencer();
        }
        private void OnSSequencerChange()
        {
            InsertSequencer();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        #region Insert Sequencer
        private List<ValueDropdownItem> _service;
        [NonSerialized] private readonly Dictionary<string, ulong> _addrMapping = new();
        [NonSerialized] private readonly Dictionary<string, string> _nameMapping = new();
        private void InsertSequencer()
        {
            _service = Router?.GetServices();
            if (_service == null) return;
            _addrMapping.Clear();
            _nameMapping.Clear();
            
            //添加部分预设Step
            _service.Add(new ValueDropdownItem("Seq Ctrl/Concurrent", new ConcurrentSteps()));
            _service.Add(new ValueDropdownItem("Seq Ctrl/Exit", new ExitStep()));
            _service.Add(new ValueDropdownItem("Seq Ctrl/Related Jump", new RelatedJumpStep()));
            _service.Add(new ValueDropdownItem("Seq Ctrl/Absolute Jump", new AbsoluteJumpStep()));
            
            //通用 Debug 用
            _service.Add(new ValueDropdownItem("Seq Debug/Debug.Log", new SSequenceDebug()));
            
            //剔除非可用选项
            for (var index = _service.Count - 1; index >= 0; index--)
            {
                var service = _service[index];
                var isStep = service.Value is ISSequenceStep;
                if (isStep)
                {
                    if (service.Value is ISAddress isAddress && !string.IsNullOrEmpty(isAddress.Guid))
                    {
                        // Debug.Log($"? {isAddress.Guid} {isAddress.Address}");
                        _addrMapping.TryAdd(isAddress.Guid, isAddress.Address);
                        if (isAddress is ISSequenceStep isSequenceStep)
                            _nameMapping.TryAdd(isAddress.Guid, isSequenceStep.Name);
                    }
                    continue;
                }
                _service.RemoveAt(index);
            }
            
            //递归插入所有可用Step
            sSequencer.Source = _service;
            sSequencer.Recurse(InsertIntoStep);
        }
        private void InsertIntoStep(ISSequenceStep step)
        {
            //Fixing old Addr
            if (step is ISAddress isAddress)
            {
                var match = FixingAddress(isAddress);
                if (!match)
                {
                    step.Name = "Error";
                    UnityEditor.EditorUtility.SetDirty(this);
                }
                else if(_nameMapping.TryGetValue(isAddress.Guid, out var corName))
                {
                    step.Name = corName;
                }
            }
            
            //Bake Valid Condition Providers
            if (step is IConditionHolder icHolder)
            {
                IValueSource?.BakeCondition(icHolder.Condition);
            }
        }
        private bool FixingAddress(ISAddress addr)
        {
            if (string.IsNullOrEmpty(addr.Guid)) return false;
            if (!_addrMapping.TryGetValue(addr.Guid, out var correctAddr)) return false;
            if (correctAddr == addr.Address) return true;
            
            Debug.Log($"Fixing address: {addr.Guid} : {addr.Address} => {correctAddr}"); 
            addr.Address = correctAddr;
            UnityEditor.EditorUtility.SetDirty(this);
            return true;
        }

        #region Guid Fixing
        private Dictionary<string, string> _guidFixMap;
        public void FixAddress(Dictionary<string, string> mapping)
        {
            _guidFixMap = mapping;
            sSequencer.Recurse(FixGuid);
        }
        private void FixGuid(ISSequenceStep step)
        {
            if (step is not ISAddress addr) return;
            if (string.IsNullOrEmpty(addr.Guid)) return;
            if (!_guidFixMap.TryGetValue(addr.Guid, out var correctGuid)) return;
            if (string.Equals(addr.Guid, correctGuid)) return;

            Debug.Log($"Guid Fix: {addr.Guid} => {correctGuid}");
            addr.Guid = correctGuid;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endregion
        #endregion
        #region Look for Router & ValueSource
        [OnInspectorInit] private void AutoFindRouter()
        {
            if (Router != null) return;
            FindRouter();
        }
        [FoldoutGroup("Tool")][Button] private void FindRouter()
        {
            var trans = transform;
            while (trans)
            {
                if (trans.TryGetComponent(out ISAddressable isAddr) && !ReferenceEquals(isAddr, this))
                {
                    Router = isAddr;
                    break;
                }
                trans = trans.parent;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        [FoldoutGroup("Tool")][Button] private void FindValueSource()
        {
            var trans = transform;
            while (trans)
            {
                if (trans.TryGetComponent(out IValueSource ivs))
                {
                    IValueSource = ivs;
                    break;
                }
                trans = trans.parent;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endregion
        #region Gizmos & Handles
        private void OnDrawGizmosSelected()
        {
            _gizmosDrawOnCond ??= GizmosDrawCond;
            _gizmosDrawOnStep ??= GizmosDrawStep;
            EditorBuffer.ResetDefault();
            foreach (var sequence in sSequencer.dataSequences)
            {
                sequence.sequence.RecurseWithCond(_gizmosDrawOnCond, _gizmosDrawOnStep);
            }

            //可播放队列绘制
            var startDrawPos = transform.position;
            var down = -UnityEditor.SceneView.lastActiveSceneView.camera.transform.up;
            var drawList = sSequencer.dataSequences;
            UnityEditor.Handles.Label(startDrawPos + down, $"[Seq] {name}", EditorDrawing.LeftAlignStyle);
            for (var index = 0; index < drawList.Count; index++)
            {
                var draw = drawList[index];
                UnityEditor.Handles.Label(startDrawPos + (index + 2) * down, $"- {draw.sequenceEntry.name}",
                    EditorDrawing.LeftAlignStyle);
            }
        }
        [HideInInspector] public readonly BucketDictionary EditorBuffer = new();
        private Action<ICondition> _gizmosDrawOnCond;
        private Action<ISSequenceStep> _gizmosDrawOnStep;
        private void GizmosDrawCond(ICondition step)
        {
            if (step is not IGizmosDrawing igd) return;
            igd.DrawGizmos(EditorBuffer);
        }
        private void GizmosDrawStep(ISSequenceStep step)
        {
            if (step is not IGizmosDrawing igd) return;
            igd.DrawGizmos(EditorBuffer);
        }
        #endregion

        #endif
    }
}
