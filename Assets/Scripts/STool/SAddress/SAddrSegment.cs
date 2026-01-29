using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace STool.SAddress
{
    [Serializable]
    public class SAddrSegment
    {
        [HorizontalGroup] public int offset;
        [HorizontalGroup] [Min(1)] public int width;

        private string CalculateCapacity()
        {
            return $"Capacity: {Mathf.Pow(2, width)}\tNext Start: {width + offset}.\n" +
                   $"Range   : {Mathf.Pow(2, offset):0} -- {Mathf.Pow(2, offset + width):0}";
        }

        public override string ToString() => $"Segment: {name}";

        [InfoBox("@CalculateCapacity()")]
        public string name;
        public string description;
        public ulong GetMask() => SAddrBitUtility.MakeMask(offset, width);
    }
}
