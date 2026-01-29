using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.SConditions
{
    [Serializable]
    public sealed class AndCondition : ICondition
    {
        [HideReferenceObjectPicker]
        [SerializeReference] public List<ICondition> items = new();

        public bool Evaluate(in ConditionContext ctx)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var c = items[i];
                if (c != null && !c.Evaluate(in ctx))
                    return false;
            }
            return true;
        }

        public void Recursion(Action<ICondition> onEachCond)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var c = items[i];
                c.Recursion(onEachCond);
            }
        }
    }

    [Serializable]
    public sealed class OrCondition : ICondition
    {
        [HideReferenceObjectPicker]
        [SerializeReference] public List<ICondition> items = new();

        public bool Evaluate(in ConditionContext ctx)
        {
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var c = items[i];
                if (c != null && c.Evaluate(in ctx))
                    return true;
            }
            return false;
        }
        
        public void Recursion(Action<ICondition> onEachCond)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var c = items[i];
                c.Recursion(onEachCond);
            }
        }
    }

    [Serializable]
    public sealed class OrThenCondition : ICondition
    {
        [HideReferenceObjectPicker]
        [SerializeReference] public List<ICondition> items = new();
        [SerializeField] private int count = 1;
        
        public bool Evaluate(in ConditionContext ctx)
        {
            var cnt = 0;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var c = items[i];
                if (c != null && c.Evaluate(in ctx)) cnt++;
                if (cnt >= count) return true;
            }
            return false;
        }
        
        public void Recursion(Action<ICondition> onEachCond)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var c = items[i];
                c.Recursion(onEachCond);
            }
        }
    }

    [Serializable]
    public sealed class NotCondition : ICondition
    {
        [HideReferenceObjectPicker]
        [SerializeReference] public ICondition inner;
        public bool Evaluate(in ConditionContext ctx) => inner == null || !inner.Evaluate(in ctx);
        
        public void Recursion(Action<ICondition> onEachCond)
        {
            inner.Recursion(onEachCond);
        }
    }
}