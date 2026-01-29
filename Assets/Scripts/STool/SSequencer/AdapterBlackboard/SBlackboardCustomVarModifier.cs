using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SAddress;
using STool.SBlackBoard;
using STool.SBlackBoard.Unity;
using STool.SConditions;
using STool.SConditions.BlackboardAdapter;
using UnityEngine;

namespace STool.SSequencer.AdapterBlackboard
{
    public class SBlackboardCustomVarModifier : SerializedMonoBehaviour, ISBlackboardBakeTarget
        , ISAddressable
        , ISSequenceStepDealer
    {
        #region Addressable Attributes
        [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][SerializeField] 
        private ulong address;
        [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][SerializeField] 
        private string guid = System.Guid.NewGuid().ToString();
        public ulong Address
        {
            get => address;
            set => address = value;
        }
        public string Guid
        {
            get => guid;
            set => guid = value;
        }
        #endregion
        #region ValueSrc
        [OdinSerialize] private IValueSource _valueSource;
        public IValueSource ValueSource
        {
            get => _valueSource;
            set => _valueSource = value;
        }
        #endregion
        #region Value Modifiers
        [HideReferenceObjectPicker][OnValueChanged("InsertProvider", true)]
        [OdinSerialize] private List<ModifySlot> _modifySlots = new();
        [Button] public void ModifyValue()
        {
            if (ValueSource == null) return;
            foreach (var mod in _modifySlots)
            {
                mod.DoModify(ValueSource);
                Debug.Log($"Modify => {mod.entry.Name}");
            }
        }

        #region Abstract
        [Serializable] public abstract class ModifySlot
        {
            [ValueDropdown("EntrySrc")][HorizontalGroup("0")][HideLabel]
            [SerializeReference] public Entry entry;
            public abstract void DoModify(IValueSource source);
            public abstract Type GetValueType();
            [HideInInspector]
            public List<ValueDropdownItem> EntrySrc;
        }
        [Serializable] public class ModifySlotT<T> : ModifySlot
        {
            [HorizontalGroup("0")][InlineProperty][LabelText("$OperatorText")]
            [SerializeReference][HideReferenceObjectPicker] public BBValue<T> value;
            public override void DoModify(IValueSource source) { }
            public override Type GetValueType() => typeof(T);
            protected virtual string OperatorText => "";
        }
        #endregion
        #region Operators
        [Serializable] public sealed class ModIntSet : ModifySlotT<int>
        {
            public override void DoModify(IValueSource source)
            {
                source.Set(entry.Id, value.Value);
            }
            protected override string OperatorText => "   =";
        }
        [Serializable] public sealed class ModIntAdd : ModifySlotT<int>
        {
            public override void DoModify(IValueSource source)
            {
                source.TryGet(entry.Id, out int formerVal);
                source.Set(entry.Id, formerVal + value.Value);
            }
            protected override string OperatorText => "   +";
        }
        [Serializable] public sealed class ModFloatSet : ModifySlotT<float>
        {
            public override void DoModify(IValueSource source)
            {
                source.Set(entry.Id, value.Value);
            }
            protected override string OperatorText => "   =";
        }
        [Serializable] public sealed class ModFloatAdd : ModifySlotT<float>
        {
            public override void DoModify(IValueSource source)
            {
                source.TryGet(entry.Id, out int formerVal);
                source.Set(entry.Id, formerVal + value.Value);
            }
            protected override string OperatorText => "   +";
        }
        [Serializable] public sealed class ModBoolSet : ModifySlotT<bool>
        {
            public override void DoModify(IValueSource source)
            {
                source.Set(entry.Id, value.Value);
            }
            protected override string OperatorText => "   =";
        }

        #endregion   
        
        #endregion
        #region Router Interfaces
        public void Resolve(ISAddress add)
        {
            // Debug.Log($"[Modifier {name}][Rec]: {add.Address}");
            if (add is not SeqEvt_BlackboardValueModify) return;
            ModifyValue();
        }
        public List<ValueDropdownItem> GetServices()
        {
            var list = new List<ValueDropdownItem>()
            {
                new()
                {
                    Text = $"[Var] Mod {name}",
                    Value = new SeqEvt_BlackboardValueModify()
                    {
                        Address = Address,
                        Guid = Guid,
                        Name = gameObject.name
                    }
                }
            };
            return list;
        }
        public string Name { get; set; }
        #endregion
        #region Seq Events
        // ReSharper disable once InconsistentNaming
        [Serializable] public sealed class SeqEvt_BlackboardValueModify : ISAddress, ISSequenceStep
        {
            #region Addressable Attributes
            [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][SerializeField] 
            private ulong address;
            [HorizontalGroup("Addr")][ReadOnly][ShowInInspector][HideLabel][SerializeField] 
            private string guid;
            public ulong Address
            {
                get => address;
                set => address = value;
            }
            public string Guid
            {
                get => guid;
                set => guid = value;
            }
            #endregion

            [SerializeField] private string name;
            public string Name
            {
                get => name;
                set => name = value;
            }

            public override string ToString() => $"[Var] Mod by {Name}";
        }
        #endregion
        #if UNITY_EDITOR
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (!definition) AutoFindBlackboard();
            if (definition) InsertProvider();
        }

        #region Auto Provider
        private void InsertProvider()
        {
            var list = definition.GetDropdownItems();
            foreach (var slot in _modifySlots)
            {
                definition.TryFixWithGuid(ref slot.entry);
                slot.EntrySrc = list.FindAll(addi =>
                    addi.Value is Entry { ValueType: not null } e && e.ValueType == slot.GetValueType());
            }
        }

        #endregion
        #region Auto Bake
        [FoldoutGroup("Tool")] [SerializeReference]
        [HorizontalGroup("Tool/0")]
        private SBlackboardDefinition definition;
        
        [FoldoutGroup("Tool")] [Button]
        [HorizontalGroup("Tool/0")]
        private void AutoFindBlackboard()
        {
            var trans = transform;
            do
            {
                if (trans.TryGetComponent(out SBlackboardMono sbMono) && sbMono.definition)
                {
                    definition = sbMono.definition;
                    break;
                }

                trans = trans.parent;
            } while (trans);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endregion
        #endif
    }
}
