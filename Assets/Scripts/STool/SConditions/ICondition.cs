using System;

namespace STool.SConditions
{
    public interface ICondition
    {
        bool Evaluate(in ConditionContext ctx);
        void Recursion(Action<ICondition> onEachCond) => onEachCond(this);
    }

    public interface IConditionHolder
    {
        ICondition Condition { get; }
    }
}