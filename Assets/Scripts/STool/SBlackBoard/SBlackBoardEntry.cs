#nullable enable
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.STag;
using UnityEngine;

namespace STool.SBlackBoard
{
    public interface IEntryHolder
    {
        [ValueDropdown("ValueProviders")]
        public Entry Entry { get; set; }
        #if UNITY_EDITOR
        public List<ValueDropdownItem> ValueProviders { get; set; }
        #endif
    }
    
    [Serializable] public sealed class Entry : ISTagHolder
    {
        [HideInInspector]
        [SerializeField] private string id = "";

        [HorizontalGroup("Top")]
        [LabelText("Name")]
        [HideLabel]
        [Tooltip(tooltip: "@tooltip")]
        [SerializeField] public string name = "NewVar";

        [FoldoutGroup("Meta")]
        [LabelText("Tooltip")]
        [ShowIf("editToolTip")]
        [TextArea(1, 3)]
        [SerializeField] public string tooltip = "";

        // The core: polymorphic default value, drawn inline.
        [HorizontalGroup("Top")]
        [OdinSerialize, SerializeReference]
        [ShowIf("showValue")]
        [HideLabel]
        [InlineProperty]
        [HideReferenceObjectPicker] // hides the small object field, keeps the inline UI clean
        public IBBValue? defaultValue;

        [OdinSerialize]
        [ValueDropdown("@AgentTags.GetBlackboardTags()")]
        public HashSet<STagData> _sTagData = new();
        
        public string Id => id;
        public string Name => name;
        public string Tooltip => tooltip;
        public ICollection<STagData> TagData { get => _sTagData; set{} }

            
        [HideInInspector]
        public bool editToolTip;
        [HideInInspector]
        public bool showValue = true;

        public IBBValue? DefaultValue => defaultValue;
        public Type? ValueType => defaultValue?.ValueType;
        
        public Entry()
        {
            // Ensure new entries have stable identity
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("N");

            // Provide a sane default value type so the row isn't "empty"
            // defaultValue ??= new BBFloat();
        }

        public Entry GetCopy()
        {
            return new Entry()
            {
                id = id,
                name = name,
                tooltip = tooltip,
                defaultValue = defaultValue,
                _sTagData = new HashSet<STagData>(_sTagData)
            };
        }

        public void Copy(Entry entry)
        {
            id = entry.id;
            name = entry.Name;
            tooltip = entry.Tooltip;
            defaultValue = entry.DefaultValue;
            _sTagData = new HashSet<STagData>(entry._sTagData);
        }

        public override string ToString()
        {
            return $"{name}, {tooltip}, {defaultValue?.ValueType.Name}, Tags: {_sTagData.Count}";
        }

#if UNITY_EDITOR
        // [Button("Regenerate Id", ButtonSizes.Small)]
        private void RegenerateId()
        {
            id = Guid.NewGuid().ToString("N");
        }

        [ContextMenu("PrintEntry")]
        private void PrintEntry()
        {
            Debug.Log($"{this}\n{Id}");
        }
#endif
    }
}