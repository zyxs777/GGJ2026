using Global;
using UnityEngine;

namespace Agent
{
    public sealed class EnemyIdentification : MonoBehaviour
    {
        #region Mono
        private void OnEnable()
        {
            GlobalShare.EventBus.Publish(new LevelRoot.LevelEvt_RegisterEnemy(){Enemy = gameObject});
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Publish(new LevelRoot.LevelEvt_UnregisterEnemy(){Enemy = gameObject});
        }

        #endregion
        
        
    }
}
