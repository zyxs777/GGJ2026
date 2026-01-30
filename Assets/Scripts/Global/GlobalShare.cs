using STool.STypeEventBus;
using UnityEngine;

namespace Global
{
    public static class GlobalShare
    {
        public static TypeEventBus EventBus = new();
        public static Collider[] Colliders = new Collider[256];

        public static void Reset()
        {
            EventBus.Reset();
        }
    }
}
