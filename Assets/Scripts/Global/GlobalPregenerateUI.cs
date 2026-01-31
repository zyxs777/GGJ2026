using UnityEngine;

namespace Global
{
    public sealed class GlobalPregenerateUI : MonoBehaviour
    {
        public GameObject[] uiPrefabs;
        private void Awake()
        {
            foreach (var prefab in uiPrefabs)
            {
                if(prefab)Instantiate(prefab, transform);
            }
        }
    }
}
