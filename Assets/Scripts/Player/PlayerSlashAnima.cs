using System;
using FMOD.Studio;
using FMODUnity;
using Global;
using UnityEngine;
using Random = UnityEngine.Random;

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
            RuntimeManager.PlayOneShot(attackSound);
            if (Random.Range(0, 100) > 50)
            {
                animator.Play(slashAnimName);
            }
            else
            {
                animator.Play("PlayerSlash2");
            }
            
        }
        public struct PlayerAnimaSlash { }
    }
}
