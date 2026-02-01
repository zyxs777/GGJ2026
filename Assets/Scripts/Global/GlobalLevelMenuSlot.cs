using TMPro;
using UnityEngine;

namespace Global
{
    public sealed class GlobalLevelMenuSlot : MonoBehaviour
    {
        [SerializeReference] private TextMeshProUGUI textMeshPro;
        [SerializeReference] private GameObject prefab;
        public void Init(string text, GameObject prefab)
        {
            textMeshPro.text = text;
            this.prefab = prefab;
        }

        public void StartLevel()
        {
            GlobalShare.EventBus.Publish(new GlobalLerpUI.UIEvtLerp()
            {
                EnterTime = 3,
                OnLerpMiddle = () => { Instantiate(prefab); }
            });
            
        }
    }
}
