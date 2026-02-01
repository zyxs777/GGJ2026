using System;
using Global;
using UnityEngine;
using Random = UnityEngine.Random;

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
