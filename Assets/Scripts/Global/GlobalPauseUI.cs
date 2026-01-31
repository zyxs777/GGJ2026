using Sirenix.OdinInspector;
using UnityEngine;

namespace Global
{
    public sealed class GlobalPauseUI : MonoBehaviour
    {
        #region Mono
        private void Awake()
        {
            GlobalShare.EventBus.Subscribe<Global_GamePause>(OnGlobalPause);
            GlobalShare.EventBus.Subscribe<Global_GameResume>(OnGlobalResume);
            uiRoot.SetActive(false);
        }
        
        #endregion

        #region System Act

        public void DoResume()
        {
            GlobalShare.EventBus.Publish(new Global_GameResume());
        }

        public void DoExit()
        {
            GlobalShare.EventBus.Publish(new Global_GameResume());
            GlobalShare.EventBus.Publish(new Global_ExitLevel());
        }
        #endregion
        
        #region UI
        [FoldoutGroup("UI")] [SerializeReference] private CanvasGroup canvasGroup;
        [FoldoutGroup("UI")] [SerializeReference] private GameObject uiRoot;
        private void OnGlobalPause(Global_GamePause evt)
        {
            canvasGroup.interactable = true;
            uiRoot.SetActive(true);
        }

        private void OnGlobalResume(Global_GameResume evt)
        {
            canvasGroup.interactable = false;
            uiRoot.SetActive(false);
        }
        #endregion
        
    }
}
