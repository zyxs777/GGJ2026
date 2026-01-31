using STool.CollectionUtility;
using STool.STypeEventBus;
using UnityEngine;

namespace Global
{
    public static class GlobalShare
    {
        #region GlobalTime
        public static readonly DecoratedValue<float> GlobalTime = new(1, OnGlobalTimeChange);
        private static void OnGlobalTimeChange(float timeScale)
        {
            GlobalTimeDelta = Time.fixedDeltaTime * timeScale;
        }
        public static float GlobalTimeDelta { get; private set; }

        #endregion        
        
        public static readonly TypeEventBus EventBus = new();
        public static Collider[] Colliders = new Collider[256];

        public static void Reset()
        {
            GlobalTime.Clear();
            GlobalTime.OnValueChanged = OnGlobalTimeChange;
            GlobalTimeDelta = Time.fixedDeltaTime;
            GlobalTime.Recompute();
            
            EventBus.Reset();
        }


        #region Camera
        public static Camera MainCamera;

        #endregion
        
        #region Cursor

        public static void CenterCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;
        }

        #endregion
    }
}
