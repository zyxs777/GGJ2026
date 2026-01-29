using System;

namespace STool.SConditions.BlackboardAdapter.CommonCond
{
    [Serializable]
    public sealed class TimeReached : ICondition
    {
        public float At;
        public bool Evaluate(in ConditionContext ctx) => ctx.Now >= At;
        public void Recursion(Action<ICondition> onEachCond) => onEachCond(this);
    }
}