using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using STool.SConditions;
using UnityEngine;

namespace STool.SSequencer
{
    #region 序列播放器默认步骤
    [Serializable] public sealed class SSequenceStep : ISSequenceStep
        , ISSequenceStepUser
        , IConditionHolder
    {
        //指令实际内容
        [HideReferenceObjectPicker]
        [SerializeReference] 
        [TableColumnWidth(width:80)]
        public ICondition gate = new AndCondition(){ items = {new SSeqTimeReach()}};        //门控

        [ValueDropdown(nameof(Source))]
        [SerializeReference] [HideLabel]
        public ISSequenceStep step;    
        //执行内容

        public string Name
        {
            get => nameof(SSequenceStep);
            set { }
        }

        public void Recurse(Action<ISSequenceStep> doOnStep)
        {
            step?.Recurse(doOnStep);
            doOnStep(this);
        }

        public void Execute(SequenceContext sequenceContext)
        {
            step.Execute(sequenceContext);
        }
        public List<ValueDropdownItem> Source { get; set; }
        public ICondition Condition => gate;
    }
    #endregion
    #region 并发步骤
    [Serializable] public sealed class ConcurrentSteps : ISSequenceStep, ISSequenceStepUser
    {
        [SerializeReference] 
        [ValueDropdown(nameof(Source))]
        // [ListDrawerSettings(ListElementLabelName = "@((($property.ValueEntry.WeakSmartValue) as ISSequenceStep)?.Name)")]
        public List<ISSequenceStep> multSteps = new();

        public string Name
        {
            get => nameof(ConcurrentSteps);
            set { }
        }

        public void Recurse(Action<ISSequenceStep> doOnStep)
        {
            foreach (var step in multSteps)
                step?.Recurse(doOnStep);
            doOnStep(this);
        }

        public void Execute(SequenceContext sequenceContext)
        {
            var player = sequenceContext.Player;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < multSteps.Count; i++)
            {
                multSteps[i].Execute(sequenceContext);
                player.Dealer.Handle(multSteps[i]);
                // Debug.Log($"Step Deal {i} {multSteps[i]}");
            }
        }

        public List<ValueDropdownItem> Source { get; set; }
        public override string ToString() => $"Concurrent Steps";
    }
    #endregion
    #region 关闭步骤
    /// <summary>
    /// 触发时清空当前所有序列内容
    /// </summary>
    [Serializable] public sealed class ExitStep : ISSequenceStep
    {
        public string Name { get; set; }
        public void Execute(SequenceContext sequenceContext)
        {
            sequenceContext.Player.Reset();
        }
        public override string ToString() => $"Exit";
    }
    

    #endregion
    #region 跳转
    /// <summary>
    /// 相对时间跳转，仅对包含ReachTime的有效，后跳时略过，前跳时恢复
    /// </summary>
    [Serializable] public sealed class RelatedJumpStep : ISSequenceStep, ISStepSpan
    {
        [SerializeField] private float jumpTime = -1;
        
        public string Name { get; set; }
        public void Execute(SequenceContext sequenceContext)
        {
            var player = sequenceContext.Player;
            var curTime = player.Time;
            var arrTime = curTime + jumpTime;

            var jumpFuture = jumpTime >= 0;
            var fromList = jumpFuture ? player.RuntimeSeq : player.PlayedSeq;
            var toList = jumpFuture ? player.PlayedSeq : player.RuntimeSeq;
            
            for (var i = fromList.Count - 1; i >= 0; i--)
            {
                var seq = fromList[i];
                if (!SSequenceUtility.TryGetStepTime(seq, out var stepTime)) continue;
                if (!SSequenceUtility.IsBetween(curTime, arrTime, stepTime)) continue;
                fromList.Remove(seq);
                toList.Add(seq);
            }

            player.Time = arrTime;
        }



        public override string ToString() => $"Related Jump {(jumpTime >= 0 ? "+" : "")}{jumpTime}";

        public float GetStart() => 0;
        public float GetDuration() => jumpTime;
    }

    /// <summary>
    /// 绝对时间跳转，仅对包含ReachTime的有效，后跳时略过，前跳时恢复
    /// </summary>
    [Serializable] public sealed class AbsoluteJumpStep : ISSequenceStep
    {
        [SerializeField] private float jumpTime = 1;
        
        public string Name { get; set; }
        public void Execute(SequenceContext sequenceContext)
        {
            var player = sequenceContext.Player;
            var curTime = player.Time;
            var arrTime = jumpTime;

            var jumpFuture = arrTime >= curTime;
            var fromList = jumpFuture ? player.RuntimeSeq : player.PlayedSeq;
            var toList = jumpFuture ? player.PlayedSeq : player.RuntimeSeq;
            
            for (var i = fromList.Count - 1; i >= 0; i--)
            {
                var seq = fromList[i];
                if (!SSequenceUtility.TryGetStepTime(seq, out var stepTime)) continue;
                if (!SSequenceUtility.IsBetween(curTime, arrTime, stepTime)) continue;

                fromList.Remove(seq);
                toList.Add(seq);
            }
            
            player.Time = arrTime;
        }

        public override string ToString() => $"Jump {jumpTime}";
    }
    #endregion
}
