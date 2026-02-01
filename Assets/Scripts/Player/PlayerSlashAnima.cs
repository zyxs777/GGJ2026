using System;
using FMOD.Studio;
using FMODUnity;
using Global;
using UnityEngine;

namespace Player
{
    public sealed class PlayerSlashAnima : MonoBehaviour
    {
        [SerializeReference] private Animator animator;
        [SerializeField] private string slashAnimName;
        [SerializeField] private EventReference attackSound;
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
            RuntimeManager.PlayOneShot(attackSound);
        }
        public struct PlayerAnimaSlash { }
    }
}
