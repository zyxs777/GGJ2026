using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using STool.SBlackBoard;
using STool.STag;
using UnityEngine;

namespace STool.SConditions.BlackboardAdapter.CommonCond
{
    // ReSharper disable once InconsistentNaming
    [Serializable] public sealed class Int_LargeOrEqual : ICondition, IEntryHolder
    {
        [HideLabel] [HorizontalGroup] [ValueDropdown(nameof(ValueProviders))] [SerializeReference]
        private Entry entry = new() { showValue = false };
        [HorizontalGroup] [SerializeField] [LabelText(">=")]
        public int threshold;

        public bool Evaluate(in ConditionContext ctx)
        {
            if (!ctx.Source.TryGet(entry.Id, out int v)) return false;
            return v >= threshold;
        }
        public void Recursion(Action<ICondition> onEachCond) => onEachCond(this);
        public Entry Entry
        {
            get => entry;
            set
            {
                entry = value;
                entry.showValue = false;
                entry.editToolTip = false;
            }
        }
        private List<ValueDropdownItem> _valueDropdownItems;
        public List<ValueDropdownItem> ValueProviders
        {
            get => _valueDropdownItems?
                .Where(item => item.Value is Entry { DefaultValue: BBValue<int> })
                .ToList() ?? throw new InvalidOperationException();
            set => _valueDropdownItems = value; 
        }

        public override string ToString() => $"{entry.name} >= {threshold}";
    }
    
    // ReSharper disable once InconsistentNaming
    [Serializable] public sealed class Int_Less : ICondition, IEntryHolder
    {
        [HideLabel] [HorizontalGroup] [ValueDropdown(nameof(ValueProviders))] [SerializeReference]
        private Entry entry = new() { showValue = false };
        [HorizontalGroup] [SerializeField] [LabelText("<")]
        public int threshold;

        public bool Evaluate(in ConditionContext ctx)
        {
            if (ctx.Source.TryGet(entry.Id, out int v))
            {
                return v < threshold;
            }
            return false;
        }
        public void Recursion(Action<ICondition> onEachCond) => onEachCond(this);
        public Entry Entry
        {
            get => entry;
            set
            {
                entry = value;
                entry.showValue = false;
                entry.editToolTip = false;
            }
        }
        private List<ValueDropdownItem> _valueDropdownItems;
        public List<ValueDropdownItem> ValueProviders
        {
            get => _valueDropdownItems?
                .Where(item => item.Value is Entry { DefaultValue: BBValue<int> })
                .ToList() ?? throw new InvalidOperationException();
            set => _valueDropdownItems = value; 
        }

        public override string ToString() => $"{entry.name} < {threshold}";
    }
}
