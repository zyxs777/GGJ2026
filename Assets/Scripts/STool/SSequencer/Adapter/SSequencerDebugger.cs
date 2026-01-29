using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using STool.SAddress;
using UnityEngine;

namespace STool.SSequencer.Adapter
{
    public class SSequencerDebugger : SerializedMonoBehaviour,
        ISAddressable,
        ISSequenceStepDealer
    {
        [SerializeField] private ulong address;
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();

        public ulong Address
        {
            get => address;
            set => address = value;
        }
        public string Guid
        {
            get => guid;
            set => guid = value;
        }

        public void Resolve(ISAddress add)
        {
            if (add is not Seq_DebugCmd cmd) return;
            Debug.Log($"[Seq Debug] {name} Rec {cmd}");
        }
        
        public List<ValueDropdownItem> GetServices()
        {
            var list = new List<ValueDropdownItem>();
            list.Add(new ValueDropdownItem(){Text = "Seq Debug/Seq_DebugCmd", Value = new Seq_DebugCmd()
            {
                Address = Address,
                Guid = Guid,
                Name = name
            }});
            return list;
        }

        #region DebugSeq
        // ReSharper disable once InconsistentNaming
        [Serializable] public sealed class Seq_DebugCmd : ISSequenceStep, ISAddress
        {
            [SerializeField] private ulong address;
            [SerializeField] private string guid;
            public string Name { get; set; }

            public ulong Address
            {
                get => address;
                set => address = value;
            }

            public string Guid
            {
                get => guid;
                set => guid = value;
            }

            public override string ToString() => $"[Debug] {Name}";
        }
        

        #endregion
    }
}
