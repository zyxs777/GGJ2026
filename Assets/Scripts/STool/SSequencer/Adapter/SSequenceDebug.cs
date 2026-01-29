using System;
using UnityEngine;

namespace STool.SSequencer.Adapter
{
    [Serializable] public sealed class SSequenceDebug : ISSequenceStep
    {
        public string Name { get; set; }
        [SerializeField] private string debugInfo = "Debugging";
        public void Execute(SequenceContext sequenceContext)
        {
            var player = sequenceContext.Player;
            Debug.Log($"[Seq Debug] {debugInfo}" +
                      $"\n[{player.Time}:{player.RuntimeSeq.Count}=>{player.PlayedSeq.Count}]");
        }
    }
}
