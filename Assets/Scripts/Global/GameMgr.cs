using UnityEngine;

namespace Global
{
    public sealed class GameMgr : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GlobalShare.Reset();

        }
        
    }
}
