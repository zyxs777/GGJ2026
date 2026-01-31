using STool.SInterfaces;
using UnityEngine;

namespace Global
{
    public sealed class GlobalShareSO : ScriptableObject, ITimeSource
    {
        public float TimeDelta { get=>GlobalShare.GlobalTimeDelta; set{} }
        public float TimeScale { get=>GlobalShare.GlobalTime.Value; set{} }
    }
}
