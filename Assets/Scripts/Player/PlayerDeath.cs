using System;
using FMODUnity;
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

        [SerializeField] private EventReference deathSound;
        [SerializeReference] private Image deathImg;
        private Action<PlayerEvtDeath> _onDeath;
        private void OnPlayerDeath(PlayerEvtDeath evt)
        {
            GlobalShare.EventBus.Unsubscribe(_onDeath);
            RuntimeManager.PlayOneShot(deathSound);
            Tween.Color(deathImg, Color.red, .2f, Ease.InBounce).OnComplete(() =>
                {
                    Tween.Color(deathImg, Color.black, 2, Ease.OutBounce).OnComplete(() =>
                    {
                        GlobalShare.EventBus.Publish(new Global_ExitLevel());
                    });
                }
            );
        }
        public struct PlayerEvtDeath { }
    }
}
