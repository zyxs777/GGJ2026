#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using STool.CollectionUtility;
using STool.SConditions;
using STool.SInterfaces;
using UnityEditor;

namespace STool.SSequencer.Adapter
{
    [CustomEditor(typeof(SAddressableSequencer))]
    public class SAddressableSequencerEditor : OdinEditor
    {
        private void OnSceneGUI()
        {
            var comp = target as SAddressableSequencer;
            if (!comp) return;

            _doOnStepAction ??= DoOnStep;
            _doOnCondAction ??= DoOnCond;
            _bucketDictionary = comp.EditorBuffer;
            _bucketDictionary.Clear();
            var sequencer = comp.sSequencer.dataSequences;
            
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(comp, "Change Sequencer");

            foreach (var sequenceSlot in sequencer)
            {
                sequenceSlot.sequence.RecurseWithCond(_doOnCondAction, _doOnStepAction);
            }

            if (!EditorGUI.EndChangeCheck()) return;

            EditorUtility.SetDirty(comp);
        }

        #region Draw Handles for EachSlot
        private BucketDictionary _bucketDictionary;
        private Action<ICondition> _doOnCondAction;
        private Action<ISSequenceStep> _doOnStepAction;

        private void DoOnCond(ICondition cond)
        {
            if (cond is not IHandlesDrawing ihd) return;
            ihd.DrawHandles(_bucketDictionary);
        }
        private void DoOnStep(ISSequenceStep step)
        {
            if (step is not IHandlesDrawing ihd) return;
            ihd.DrawHandles(_bucketDictionary);
        }
        

        #endregion
        
    }
}
#endif
