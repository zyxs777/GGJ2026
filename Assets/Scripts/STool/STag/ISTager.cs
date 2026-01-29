using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.STag
{
    public interface ISTagHolder
    {
        public ICollection<STagData> TagData { get; set; }
    }

    [Serializable]
    public record STagData
    {
        [PropertyTooltip("$description")]
        public string name;
        
        [ShowIf("editDesc")]
        public string description;

        [HideInInspector]
        public bool editDesc;
    }
}
