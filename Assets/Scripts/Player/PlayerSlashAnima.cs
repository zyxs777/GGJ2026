using System;
using Global;
using UnityEngine;

namespace Player
{
    public sealed class PlayerSlashAnima : MonoBehaviour
    {
        [SerializeReference] private Animator animator;
        [SerializeField] private string slashAnimName;

        private void OnEnable()
        {
            _onRecAnima ??= OnRecAnima;
            GlobalShare.EventBus.Subscribe(_onRecAnima);
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onRecAnima);
        }

        private Action<PlayerAnimaSlash> _onRecAnima;
        private void OnRecAnima(PlayerAnimaSlash evt)
        {
            animator.Play(0);
            animator.Play(slashAnimName);
        }
        public struct PlayerAnimaSlash { }
    }
}
