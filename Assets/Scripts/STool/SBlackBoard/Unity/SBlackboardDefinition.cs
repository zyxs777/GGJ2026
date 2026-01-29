#nullable enable
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.STag;
using UnityEngine;
// ReSharper disable ForCanBeConvertedToForeach

namespace STool.SBlackBoard.Unity
{
    /// <summary T="subclasses.">
    /// Editor-facing blackboard schema (variables + default values).
    /// - Uses Odin serialization so polymorphic default values show & persist in Inspector.
    /// - No per-type "Add" buttons: extend by adding new BBValue
    /// </summary>
    [CreateAssetMenu(menuName = "STool/Blackboard/Blackboard Definition", fileName = "BB_Definition")]
    public sealed class SBlackboardDefinition : SerializedScriptableObject
    {
        [BoxGroup("Inheritance")]
        [LabelText("Parent Definition")]
        [SerializeField] private SBlackboardDefinition? parent;

        [BoxGroup("Variables")]
        [OdinSerialize]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, HideAddButton = false, HideRemoveButton = false, NumberOfItemsPerPage = 50)]
        [HideReferenceObjectPicker]
        private List<Entry> _entries = new();

        public SBlackboardDefinition? Parent
        {
            get => parent;
            set => parent = value;
        }
        public List<Entry> Entries => _entries;

        /// <summary>Find entry by id, searching parent chain.</summary>
        public bool TryFind(string id, out Entry entry)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (!string.Equals(_entries[i].Id, id, StringComparison.Ordinal)) continue;
                entry = _entries[i];
                return true;
            }

            if (parent != null)
                return parent.TryFind(id, out entry);

            entry = null!;
            return false;
        }

        /// <summary>
        /// Enumerate entries with parent chain (parent first, then local).
        /// Best practice: keep ID unique across the chain.
        /// </summary>
        public IEnumerable<Entry> EnumerateAll()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            if (parent != null)
            {
                foreach (var e in parent.EnumerateAll())
                {
                    if (seen.Add(e.Id))
                        yield return e;
                }
            }

            for (var i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (seen.Add(e.Id))
                    yield return e;
            }
        }
        public void SetBlackBoard(Blackboard? blackboard)
        {
            if (blackboard == null) return;
            for (var i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                e.DefaultValue?.InsertKey(blackboard, e.Id);
            }
        }
        
        #if UNITY_EDITOR
        [FoldoutGroup("Tools")]
        [Button("Validate (Ids / Types)", ButtonSizes.Medium)]
        private void ValidateNow()
        {
            // Simple validation: duplicate ids within local list
            var set = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < _entries.Count; i++)
            {
                var id = _entries[i].Id;
                if (string.IsNullOrWhiteSpace(id))
                    Debug.LogWarning($"[BlackboardDefinition] Entry #{i} has empty Id.", this);
                else if (!set.Add(id))
                    Debug.LogError($"[BlackboardDefinition] Duplicate Id in local list: {id}", this);

                if (_entries[i].DefaultValue == null)
                    Debug.LogError($"[BlackboardDefinition] Entry '{_entries[i].Name}' ({id}) has null DefaultValue.", this);
            }
        }

        [FoldoutGroup("Tools")]
        [Button("Edit ToolTips", ButtonSizes.Medium)]
        private void EditToolTips(bool edit = true)
        {
            foreach (var e in _entries) e.editToolTip = edit;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        private readonly List<ValueDropdownItem> _dropdownItems = new();
        public List<ValueDropdownItem> GetDropdownItems()
        {
            _dropdownItems.Clear();
            foreach (var e in _entries)
            {
                _dropdownItems.Add(new ValueDropdownItem(e.Name, e.GetCopy()));
            }
            return _dropdownItems;
        }
        #endif


        public override string ToString()
        {
            var str = $"[Blackboard] {name} with {_entries.Count} entries";
            foreach (var e in Entries)
            {
                str += "\n" + e;
            }
            return str;
        }
    }
    
}
