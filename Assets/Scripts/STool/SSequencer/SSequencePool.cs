using STool.CollectionUtility;
using UnityEngine;

namespace STool.SSequencer
{
    public static class SSequencePool
    {
        private static readonly ReusableCollection<SSequence> SSequences = new(
            CreateSequence,
            DestroySequence,
            GetSequence,
            PushSequence);
        private static SSequence CreateSequence()=> new();
        private static void GetSequence(SSequence sSequence) => sSequence.Reset();
        private static void PushSequence(SSequence sSequence) => sSequence.Reset();
        private static void DestroySequence(SSequence sSequence)
        {
            sSequence.Dispose();
        }
        
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            SSequences.ClearToPool();    
        }

        public static void ReturnSequence(SSequence sSequence)
        {
            SSequences.Push(sSequence);
        }
        
        public static SSequence GetSequence()
        {
            return SSequences.Get();
        }
        public static SSequence GetSequence(in SSequence dataSrc)
        {
            var seq = SSequences.Get();
            seq.steps.AddRange(dataSrc.steps);
            
            #if UNITY_EDITOR
            seq.ReCalTimeLineModel();
            #endif
            
            return seq;
        }
        
    }
}
