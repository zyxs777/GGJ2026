using System;
using UnityEngine;

namespace Global
{
    public sealed class LevelRoot : MonoBehaviour
    {
        private void OnEnable()
        {
            _onLevelExit ??= OnLevelExit;
            GlobalShare.EventBus.Subscribe(_onLevelExit);
            GlobalShare.EventBus.Publish(new Global_EnterLevel());
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onLevelExit);
        }

        #region Level Exit
        private Action<Global_ExitLevel> _onLevelExit;
        private void OnLevelExit(Global_ExitLevel evt)
        {
            Destroy(gameObject);
        }
        #endregion
        
    }
}
