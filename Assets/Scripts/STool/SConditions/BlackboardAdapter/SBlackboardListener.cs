using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SBlackBoard;
using STool.SInterfaces;
using STool.STag;
using STool.STypeEventBus;
using UnityEngine;

namespace STool.SConditions.BlackboardAdapter
{
    /// <summary>
    /// 用于对黑板变量进行监听
    /// </summary>
    public sealed class SBlackboardListener : SerializedMonoBehaviour, ISBlackboardBakeTarget, IInit
    {
        #region Value Src
        [SerializeField] private SBlackboardMono blackboardMono;
        public IValueSource ValueSource
        {
            get => blackboardMono;
            set
            {
                if (value is SBlackboardMono mono) blackboardMono = mono;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }
        #endregion

        public void Initialize()
        {
            var blackboard = blackboardMono.GetBlackboard();
            foreach (var listenerUnit in ListenerUnits)
            {
                listenerUnit.Blackboard = blackboard;
                listenerUnit.OnBBValueChangedAct ??= listenerUnit.OnBBValueChanged;
                var listenAct = listenerUnit.OnBBValueChangedAct;

                //准备ID映射字典
                listenerUnit.MappingID ??= new();
                var dic = listenerUnit.MappingID;
                dic.Clear();

                BBKey tempKey = null;
                foreach (var listener in listenerUnit.listeners)
                {
                    var entry = listener.entry;
                    var key = entry.DefaultValue?.GetKey();
                    
                    if (key == null) continue;
                    key.Name = entry.Id;
                    
                    blackboard.Subscribe(key, listenAct);
                    dic.Add(listener.mappingID, entry.Id);
                    
                    tempKey = key;
                }

                if (tempKey != null) listenAct.Invoke(tempKey);
            }
        }

        #region Listeners
        private readonly CompositeDisposable _disposables = new();

        [OnValueChanged("AutoSaver", true)]
        [OnValueChanged("FillEntrySrc", true)]
        [OdinSerialize] [NonSerialized] [HideReferenceObjectPicker]
        public List<ListenerUnit> ListenerUnits = new();
        

        #endregion

        #region Editor
        #if UNITY_EDITOR
        [OnInspectorInit]
        private void FillEntrySrc()
        {
            if (Application.isPlaying) return;
            if (!blackboardMono) return;
            if (!blackboardMono.definition) return;
            
            var def = blackboardMono.definition;
            var entries = def.Entries;

            foreach (var listenerUnit in ListenerUnits)
                foreach (var listener in listenerUnit.listeners)
                    listener.entrySrc = entries;
            
        }
        
        private void AutoSaver()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
        #endregion

        #region Listeners Definition
        
        /// <summary>
        /// 监听单元
        /// </summary>
        [Serializable] public sealed class ListenerUnit
        {
            [NonSerialized] public IBlackboard Blackboard;
            [HideReferenceObjectPicker] public List<ListenerSlot> listeners = new();
            [NonSerialized] public Dictionary<string, string> MappingID = new();

            [OnValueChanged("GenerateListenerSlot")] [ShowInInspector] [SerializeField]
            public ISBlackboardListener Listener;

            public Action<BBKey> OnBBValueChangedAct;
            public void OnBBValueChanged(BBKey key) => Listener?.OnValueChange(Blackboard, MappingID);
            
            #if  UNITY_EDITOR
            [Button] private void GenerateListenerSlot()
            {
                if (Listener == null) return;
                listeners.Clear();
                var dic = Listener.TargetValues;
                foreach (var (key, value) in dic)
                {
                    listeners.Add(new ListenerSlot { entry = new Entry() { defaultValue = value }, mappingID = key });
                }
            }
            #endif
        }
        
        /// <summary>
        /// 监听映射条目
        /// </summary>
        [Serializable] public sealed class ListenerSlot
        {
            [ValueDropdown("SelectValueDropdownItems")] [HideLabel]
            [HorizontalGroup("0")] public Entry entry;
            [HorizontalGroup("0")] public string mappingID;
            
            #if UNITY_EDITOR
            [HideInInspector] public List<Entry> entrySrc;
            private List<Entry> SelectValueDropdownItems()
            {
                var valueType = entry.ValueType;
                return entrySrc?
                    .Where(item => item is { } e && e.ValueType == valueType)
                    .ToList() ?? throw new InvalidOperationException();
            }
            #endif
        }
        #endregion
    }
    
    #region Listener Interface
    public interface ISBlackboardListener
    {
        #if  UNITY_EDITOR
        public Dictionary<string, IBBValue> TargetValues { get; }
        #endif
        
        public void OnValueChange(IBlackboard blackboard, Dictionary<string, string> idMapping);
    }
    #endregion
}
