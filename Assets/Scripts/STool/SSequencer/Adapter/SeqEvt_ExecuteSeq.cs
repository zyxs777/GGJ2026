using System;
using Sirenix.OdinInspector;
using STool.SAddress;
using UnityEngine;

namespace STool.SSequencer.Adapter
{
    // ReSharper disable once InconsistentNaming
    public sealed class SeqEvt_ExecuteSeq : 
        ISAddress, 
        ISSequencePlay, 
        ISSequenceStep
    {
        [Title("$_name")]
        public SeqEvt_ExecuteSeq(ulong address, string guid ,string seqGuid, string name)
        {
            _address = address;
            _guid = guid;
            _seqGuid = seqGuid;
            _name = name;
        }
        [SerializeField][HorizontalGroup][ReadOnly][HideLabel] private ulong _address;
        [SerializeField][HorizontalGroup][ReadOnly][HideLabel] private string _guid;
        [SerializeField] private string _seqGuid;
        [SerializeField] [HideInInspector] private string _name;

        public string SeqGuid => _seqGuid;
        public ulong Address
        {
            get => _address;
            set => _address = value;
        }

        public string Guid { get=>_guid; set=> _guid = value; }
        public string Name { get => _name; set =>  _name = value; }

        
        public void Recurse(Action<ISSequenceStep> doOnStep) => doOnStep(this);
        public void Execute() { }
        public override string ToString() => $"[Seq] Play {Name}";
    }
}
