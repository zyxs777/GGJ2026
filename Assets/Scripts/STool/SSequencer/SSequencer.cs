using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using STool.SConditions;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace STool.SSequencer
{
    [Serializable]
    public class SSequencer
    {
        [ListDrawerSettings(ListElementLabelName = "@((($property.ValueEntry.WeakSmartValue) as SSequenceSlot)?.sequenceEntry.name)")]
        public List<SSequenceSlot> dataSequences = new();                                   //Data

        [ShowInInspector] [ReadOnly] [ListDrawerSettings(DefaultExpandedState = false)]
        private List<SSequence> _runtimeSequences = new();                                  //Runtime
        
        public IValueSource IValueSource;
        public ISSequenceStepDealer IStepOut;

        public void Play(string seqGuid)
        {
            //To Get the Sequence to play
            SSequence targetSeq = null;
            for (var i = dataSequences.Count - 1; i >= 0; i--)
            {
                var dSeq = dataSequences[i];
                if (!dSeq.sequenceEntry.SeqGuid.Equals(seqGuid)) continue;
                targetSeq = dSeq.sequence;
            }
            if (targetSeq == null) return;
            
            //Get sequence from pool and init
            var playSeq = SSequencePool.GetSequence(targetSeq);
            playSeq.IStepOut = IStepOut;
            playSeq.IValueSource = IValueSource;
            
            //Add to on-run list
            _runtimeSequences.Add(playSeq);
        }
        public void Update(float deltaTime)
        {
            bool doDelete;
            for (var i = 0; i < _runtimeSequences.Count; i+= doDelete ? 0 : 1)
            {
                //update per active sequence
                doDelete = false;
                var runtime = _runtimeSequences[i];
                runtime.Update(deltaTime);

                //return to pool when there is no more step in sequence
                if (!runtime.IsEmpty()) continue;

                doDelete = true;
                _runtimeSequences.RemoveAt(i);
                SSequencePool.ReturnSequence(runtime);
            }
        }
        public List<ValueDropdownItem> Source { get; set; }
        public string Name { get; set; }
        public void Recurse(Action<ISSequenceStep> doOnStep)
        {
            for (var index = 0; index < dataSequences.Count; index++)
            {
                var dataSeq = dataSequences[index];
                dataSeq.sequence.Source = Source;
                dataSeq.sequence.Recurse(doOnStep);
            }
        }
        #region Sequencer Support
        [Serializable] public sealed class SSequenceSlot
        {
            public SSequenceEntry sequenceEntry = new();
            [InlineProperty] [HideLabel] public SSequence sequence = new();
        }
        [Serializable] public sealed class SSequenceEntry
        {
            [SerializeField] [ShowInInspector] [ReadOnly]
            private string seqGuid;
            public string SeqGuid => seqGuid;
            public string name = "Seq-Name";
            public string description = "Seq-Description";

            public SSequenceEntry()
            {
                seqGuid = Guid.NewGuid().ToString();
            }
        }
        #endregion
        #region Tool
        #if UNITY_EDITOR
        [ValueDropdown("ToolGetSequence")] [FoldoutGroup("Tool")][ SerializeField] [HorizontalGroup("Tool/0")]
        private string testSequence;

        [FoldoutGroup("Tool")] [HorizontalGroup("Tool/0")] [Button]
        private void TestPlay()
        {
            Play(testSequence);
        }

        private List<ValueDropdownItem> ToolGetSequence()
        {
            var list = new List<ValueDropdownItem>();
            foreach (var dataSequence in dataSequences)
            {
                list.Add(new ValueDropdownItem(dataSequence.sequenceEntry.name, dataSequence.sequenceEntry.SeqGuid));
            }
            return list;
        }
#endif

        #endregion
    }
}
