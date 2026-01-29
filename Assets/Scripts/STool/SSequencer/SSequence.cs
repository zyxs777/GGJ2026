    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using STool.EditorExtension.SEditorExtension.Attributes;
    using STool.SConditions;
    using UnityEngine;
    
    #if UNITY_EDITOR
    using STool.EditorExtension.Timeline;

    #endif

    namespace STool.SSequencer
    {
        [Serializable]
        public sealed class SSequence : IDisposable, ISSequencePlayer, ISSequenceStepUser, ISSequenceStep
        {
#if UNITY_EDITOR
            #region Timeline View
            [TimelineView] [SerializeField] private TimelineModel timelineModels = new() { showContainers = false };
            private float _maxTime;
            private float _lastStartTime;   //补丁 目前Step那边缺失开始时间的信息
            ////目前实际可以对应timeline信息的只有ICondition中的 TimeReach
            [OnInspectorInit]
            public void ReCalTimeLineModel()
            {
                _maxTime = 0;
                timelineModels.points.Clear();
                timelineModels.spans.Clear();
                for (var index = 0; index < steps.Count; index++)
                {
                    var step = steps[index];
                    step.gate.Recursion(CalculateCond);
                    CalculateStep(step.step);
                }
                timelineModels.duration = _maxTime;
                timelineModels.points.Add(new TimelinePoint() { color = Color.white });
            }
            private void CalculateCond(ICondition cond)
            {
                float start = 0;
                switch (cond)
                {
                    case ISStepTick isStepTick:
                        start = isStepTick.GetTick();
                        timelineModels.points.Add(new TimelinePoint(time: start));
                        _maxTime = Mathf.Max(_maxTime, start);
                        break;
                    case ISStepSpan isStepSpan:
                        start = isStepSpan.GetStart();
                        var end = isStepSpan.GetStart() + isStepSpan.GetDuration();
                        timelineModels.spans.Add(new TimelineSpan(start: start, end: end));
                        _maxTime = Mathf.Max(_maxTime, end);
                        break;
                }
                _lastStartTime = start;
            }
            private void CalculateStep(ISSequenceStep step)
            {
                float start;
                switch (step)
                {
                    case ISStepTick isStepTick:
                        start = isStepTick.GetTick() + _lastStartTime;
                        timelineModels.points.Add(new TimelinePoint(time: start));
                        _maxTime = Mathf.Max(_maxTime, start);
                        break;
                    case ISStepSpan isStepSpan:
                        start = isStepSpan.GetStart() + _lastStartTime;
                        var end = start + isStepSpan.GetDuration();
                        timelineModels.spans.Add(new TimelineSpan(start: start, end: end));
                        _maxTime = Mathf.Max(_maxTime, end);
                        break;
                }
            }
            #endregion
#endif
            [OnCollectionChanged("ReCalTimeLineModel")]
            [TableList]
            [ListDrawerSettings(ListElementLabelName = "@((($property.ValueEntry.WeakSmartValue) as ISSequenceStep)?.ToString())")]
            [OnValueChanged("ReCalTimeLineModel", IncludeChildren = true)]
            [InlineProperty]
            public List<SSequenceStep> steps = new();
            private List<SSequenceStep> _playedSteps = new();
        
            [HideInInspector]
            public float time;
            public IValueSource IValueSource;
            public ISSequenceStepDealer IStepOut;
        
            public void Update(float deltaTime)
            {
                #if UNITY_EDITOR
                if (timelineModels.points.Count > 0)
                {
                    timelineModels.points[^1] = new TimelinePoint(time) { color = Color.white };
                    Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
                }
                #endif

                bool doDelete;
                for (var i = 0; i < steps.Count; i += doDelete ? 0 : 1)
                {
                    doDelete = false;
                    var step = steps[i];
                    var res = step.gate.Evaluate(new ConditionContext(IValueSource, time));
                
                    if (!res) continue;
                    
                    steps.RemoveAt(i);
                    _playedSteps.Add(step);
                    doDelete = true;
                    
                    step.Execute(new SequenceContext(this));
                    IStepOut.Handle(step.step);
                }
                time += deltaTime;
            }
            public bool IsEmpty() => steps.Count == 0;
            public ISSequenceStepDealer Dealer => IStepOut;
            
            #region SequenceContext
            public float Time { 
                get => time;
                set => time = Mathf.Max(0, value);
            }
            public List<SSequenceStep> RuntimeSeq => steps;
            public List<SSequenceStep> PlayedSeq => _playedSteps;

            #endregion
            
            public void Reset()
            {
                time = 0;
                steps.Clear();
                _playedSteps.Clear();
            }
            public void Dispose() { }
            
            public void Recurse(Action<ISSequenceStep> doOnStep)
            {
                doOnStep(this);
                foreach (var step in steps)
                {
                    step.Recurse(doOnStep);
#if UNITY_EDITOR
                    step.Recurse(InsertIntoStep);   //TODO 目前用于构建仅对此Sequence生效的作用域，让Step也可以在此目标上生效
#endif
                }
            }
            
            /// <summary>
            /// 目前用于遍历Cond和Step，用于做可视化编辑绘制
            /// </summary>
            /// <param name="doOnCond"></param>
            /// <param name="doOnStep"></param>
            public void RecurseWithCond(Action<ICondition> doOnCond, Action<ISSequenceStep> doOnStep)
            {
                foreach (var step in steps)
                {
                    if (step == null) continue;
                    step.gate.Recursion(doOnCond);
                    step.Recurse(doOnStep);
                }
            }

            #region Editor Support
            private List<ValueDropdownItem> _source;
            public List<ValueDropdownItem> Source
            {
                get => _source; 
                set => _source = new(value);
            }
            
            /// <summary>
            /// 为序列下Step提供可以执行的Step
            /// </summary>
            /// <param name="step"></param>
            private void InsertIntoStep(ISSequenceStep step)
            {
                //Bake Valid Actions
                if (_source == null) return;
                if (step is not ISSequenceStepUser isUser) return;
                isUser.Source = Source;
            }
            public string Name { get; set; }
            #endregion

        }
    }