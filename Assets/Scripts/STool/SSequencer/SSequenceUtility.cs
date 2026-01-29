using System;
using System.Runtime.CompilerServices;
using STool.SConditions;
using UnityEngine;

namespace STool.SSequencer
{
    public static class SSequenceUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(float from, float to, float value)
        {
            var min = Mathf.Min(from, to);
            var max = Mathf.Max(from, to);
            return value >= min && value <= max;
        }

        /// <summary>
        /// 这里默认Condition里直接为time或者time在组合式Condition第一位
        /// </summary>
        /// <param name="step"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStepTime(SSequenceStep step, out float time)
        {
            _tryGetStepTimeAct ??= GetStepTime;
            _stepTime = -1;
            step.Condition.Recursion(_tryGetStepTimeAct);
            time = _stepTime;
            return _stepTime > 0;
            // switch (step.Condition)
            // {
            //     case SSeqTimeReach timeReach:
            //         time = timeReach.time;
            //         return true;
            //     case AndCondition andCondition when andCondition.items.Count > 0 && andCondition.items[0] is SSeqTimeReach tr:
            //         time = tr.time;
            //         return true;
            //     case OrCondition orCondition when orCondition.items.Count > 0  && orCondition.items[0] is SSeqTimeReach tr:
            //         time = tr.time;
            //         return true;
            //     case OrThenCondition orThenCondition when orThenCondition.items.Count > 0 && orThenCondition.items[0] is SSeqTimeReach tr:
            //         time = tr.time;
            //         return true;
            //     default:
            //         time = -1;
            //         return false;
            // }
        }
        private static Action<ICondition> _tryGetStepTimeAct;
        private static float _stepTime;
        private static void GetStepTime(ICondition cond)
        {
            if (cond is SSeqTimeReach reach) _stepTime = reach.time;
        }
    }
}
