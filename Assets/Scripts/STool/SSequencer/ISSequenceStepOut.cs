using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace STool.SSequencer
{
    /// <summary>
    /// 表示可以处理Sequencer输出的Step
    /// </summary>
    public interface ISSequenceStepDealer
    {
        public virtual void Handle(ISSequenceStep step){}
    }
    
    /// <summary>
    /// 表示Sequence中的Step，即可以支持Sequencer播放的指令
    /// </summary>
    public interface ISSequenceStep
    {
        public string Name { get; set; }
        public void Recurse(Action<ISSequenceStep> doOnStep) => doOnStep(this);
        public virtual void Execute(SequenceContext sequenceContext){}
    }

    /// <summary>
    /// Step指令可继承，表示此处需要可用指令，与Sequencer组合使用
    /// </summary>
    public interface ISSequenceStepUser
    {
        public List<ValueDropdownItem> Source { get; set; }
    }
    
    /// <summary>
    /// 表示这是一条Sequencer指令，用于控制Sequencer完成指定行为
    /// </summary>
    public interface ISSequencePlay
    {
        public string SeqGuid { get; }
    }

    /// <summary>
    /// 序列播放器操作接口
    /// </summary>
    public interface ISSequencePlayer
    {
        float Time { get; set; }
        List<SSequenceStep> RuntimeSeq { get; }
        List<SSequenceStep> PlayedSeq { get; }
        public ISSequenceStepDealer Dealer { get; }
        /// <summary>
        /// 清空重置
        /// </summary>
        void Reset();
    }

    
    
    /// <summary>
    /// Runtime 下提供Sequence播放器相关接口和信息以供做Sequence相关操作
    /// </summary>
    public struct SequenceContext
    {
        public SequenceContext(ISSequencePlayer player)
        {
            Player = player;
        }
        public ISSequencePlayer Player;
    }
    
    #region 用于支持TimelineView的可视化信息提取工具
    public interface ISStepTick
    {
        public float GetTick();
    }

    public interface ISStepSpan
    {
        public float GetStart();
        public float GetDuration();
    }
    #endregion
    
    
}
