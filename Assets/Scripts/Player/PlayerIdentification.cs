using System;
using FMOD.Studio;
using FMODUnity;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Player
{
    public sealed class PlayerIdentification : MonoBehaviour
    {
        [FoldoutGroup("Setting")] [SerializeField] private int playerIndex = 0;
        [FoldoutGroup("Setting")] [SerializeField] private EventReference breatheSound;
        private EventInstance _breatheSound;
        
        private void OnEnable()
        {
            _onRequest ??= OnRequestInfo;
            GlobalShare.EventBus.Subscribe(_onRequest);
            
            _breatheSound = RuntimeManager.CreateInstance(breatheSound);
            _breatheSound.start();
        }
        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onRequest);
            _breatheSound.stop(STOP_MODE.ALLOWFADEOUT);
        }

        private Action<PlayerIDRequest> _onRequest;
        private void OnRequestInfo(PlayerIDRequest request)
        {
            GlobalShare.EventBus.Publish(new PlayerIdentificationData()
            {
                PlayerIndex = playerIndex,
                PlayerGameObject = gameObject,
            });
        }
        
        public struct PlayerIDRequest { }
        public struct PlayerIdentificationData
        {
            public int PlayerIndex;
            public GameObject PlayerGameObject;
        }
    }
}
