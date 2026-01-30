using System;
using UnityEngine;

namespace Player
{
    public sealed class PlayerMotion : MonoBehaviour
    {
        private void OnEnable()
        {
            RegisterCtrl();
        }

        private void OnDisable()
        {
            UnRegisterCtrl();
        }


        #region Rewired Registration
        private void RegisterCtrl()
        {
            var player = Rewired.ReInput.players.GetPlayer(0);
            // player.AddInputEventDelegate();
        }

        private void UnRegisterCtrl()
        {
            var player = Rewired.ReInput.players.GetPlayer(0);
            
        }

        #endregion
    }
}
