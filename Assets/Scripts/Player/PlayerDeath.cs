using System;
using Global;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public sealed class PlayerDeath : MonoBehaviour
    {
        #region Mono

        private void OnEnable()
        {
            _onDeath ??= OnPlayerDeath;
            GlobalShare.EventBus.Subscribe(_onDeath);
            deathImg.color = Color.clear;
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onDeath);
        }
        #endregion
        
        [SerializeReference] private Image deathImg;
        [SerializeReference] private RectTransform helmet;
        private Action<PlayerEvtDeath> _onDeath;
        private void OnPlayerDeath(PlayerEvtDeath evt)
        {
            if (helmet.localEulerAngles.z > 0)
            {
                GetComponent<Animator>().Play("end");
            }
            else
            {
                 GetComponent<Animator>().Play("end2");
            }
           
                           
        }

        private void End()
        {
            GlobalShare.EventBus.Publish(new Global_ExitLevel());
        }
        public struct PlayerEvtDeath { }
    }
}
