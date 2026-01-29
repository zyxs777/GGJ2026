using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using STool.SBlackBoard;
using STool.STag;
using UnityEngine;

namespace STool.SConditions.BlackboardAdapter.CommonCond
{
    [Serializable]
    public sealed class BoolIsTrue : ICondition, IEntryHolder
    {
        [HideLabel] [HorizontalGroup] [ValueDropdown(nameof(ValueProviders))] [SerializeReference]
        private Entry entry = new() { showValue = false };
        public bool Evaluate(in ConditionContext ctx)
        {
            if (!ctx.Source.TryGet(entry.Id, out bool v)) return false;
            return v == value;
        }
        
        [HorizontalGroup] [HideLabel] [SerializeField]
        private bool value = true;
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
            get => _valueDropdownItems?.Where(item => item.Value is Entry { DefaultValue: BBValue<bool> }).ToList() ?? throw new InvalidOperationException(); 
            set => _valueDropdownItems = value; 
        }
    }
}