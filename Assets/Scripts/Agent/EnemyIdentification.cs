using FMODUnity;
using Global;
using UnityEngine;

namespace Agent
{
    public sealed class EnemyIdentification : MonoBehaviour
    {
        [SerializeField] private EventReference attackSound;

        #region Mono
        private void OnEnable()
        {
            GlobalShare.EventBus.Publish(new LevelRoot.LevelEvt_RegisterEnemy(){Enemy = gameObject});
            RuntimeManager.PlayOneShot(attackSound, transform.position);

        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Publish(new LevelRoot.LevelEvt_UnregisterEnemy(){Enemy = gameObject});
        }

        #endregion
        
        
    }
}
