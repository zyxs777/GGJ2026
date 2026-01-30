using UnityEngine;

namespace Global
{
    public sealed class GameMgr : MonoBehaviour
    {
        private void Awake()
        {
            GlobalShare.Reset();
            
        }
    }
}
