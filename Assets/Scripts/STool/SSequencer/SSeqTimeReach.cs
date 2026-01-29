using System;
using STool.CollectionUtility;
using STool.SConditions;
using STool.SInterfaces;
using UnityEngine;

namespace STool.SSequencer
{
    [Serializable]
    public sealed class SSeqTimeReach : ICondition, ISStepTick, IGizmosDrawing
    {
        [SerializeField] public float time;
        public float GetTick() => time;
        
        public bool Evaluate(in ConditionContext ctx)
        {
            return time <= ctx.Now;
        }

        public void Recursion(Action<ICondition> onEachCond)
        {
            onEachCond(this);
        }
        
        #if UNITY_EDITOR
        public void DrawGizmos(BucketDictionary buffer)
        {
            buffer.Set(EditorDrawing.Time, time);
        }
        #endif
    }

    [Serializable]
    public sealed class SSeqTimeDura : ICondition, ISStepSpan
    {
        [SerializeField] private float time;
        [SerializeField] private float duration = 1;
        public float GetTick() => time;

        public bool Evaluate(in ConditionContext ctx)
        {
            return time <= ctx.Now && time + duration >= ctx.Now;
        }

        public void Recursion(Action<ICondition> onEachCond) { onEachCond(this); }
        public float GetStart() => time;
        public float GetDuration() => duration;
    }
}
