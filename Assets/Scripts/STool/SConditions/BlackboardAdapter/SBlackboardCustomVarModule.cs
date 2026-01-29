using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SBlackBoard;
using STool.STag;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

#endif

namespace STool.SConditions.BlackboardAdapter
{
    public class SBlackboardCustomVarModule : SerializedMonoBehaviour
        , ISBlackboardBakeTarget
    {
        [HideReferenceObjectPicker]
        [OnValueChanged("AutoSync", includeChildren: true)]
        [OdinSerialize] [OnCollectionChanged(Before = "OnCollectionChange", After = "AfterCollectionChange")]
        private List<Entry> _entries = new();
        
        public void DoBake(SBlackboardMono blackboardMono)
        {
            for (var index = 0; index < _entries.Count; index++)
            {
                var entry = _entries[index];
                blackboardMono.definition?.TryRegisterWithGuid(ref entry);
            }
        }

        public IValueSource ValueSource
        {
            get => _valueSource;
            set => _valueSource = value;
        }
        [OdinSerialize] private IValueSource _valueSource;

#if UNITY_EDITOR
        #region Auto Tagger
        [ValueDropdown("GetTags")] [FoldoutGroup("Tool")] [OdinSerialize]
        private List<STagData> _autoAddTags = new();
        [FoldoutGroup("Tool")] [OdinSerialize] private string _prefix = "Custom/";
        [FoldoutGroup("Tool")] [OdinSerialize] private string _filter = "AgentTags";

        private List<ValueDropdownItem> GetTags()
        {
            var list = new List<ValueDropdownItem>();
            foreach (var (label,value) in DropdownRegistry.Get<STagData>(_filter))
                list.Add(new ValueDropdownItem(label, value));
            return list;
        }

        #endregion        
        #region Auto Sync
        private void AutoSync()
        {
            if (ValueSource is not SBlackboardMono mono) return;
            DoBake(mono);
            UnityEditor.EditorUtility.SetDirty(mono);
        }
        #endregion
        #region Collection Change
        
        private void AfterCollectionChange(CollectionChangeInfo info)
        {
            if (info is { ChangeType: CollectionChangeType.Add, Value: Entry entry })
            {
                entry.name = $"{_prefix}{entry.name}";
                foreach (var sTagData in _autoAddTags)
                    entry.TagData.Add(sTagData);
            }
        }
        private void OnCollectionChange(CollectionChangeInfo info)
        {
            if (ValueSource is not SBlackboardMono mono) return;
            if (info is { ChangeType: CollectionChangeType.RemoveIndex})
            {
                var entry = _entries[info.Index];
                mono.definition?.TryUnregisterWithGuid(ref entry);
            }
        }
        #endregion
        #endif
    }
}
