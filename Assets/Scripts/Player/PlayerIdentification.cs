using System;
using Global;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player
{
    public sealed class PlayerIdentification : MonoBehaviour
    {
        [FoldoutGroup("Setting")] [SerializeField] private int playerIndex = 0;
        private void OnEnable()
        {
            _onRequest ??= OnRequestInfo;
            GlobalShare.EventBus.Subscribe(_onRequest);
        }
        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onRequest);
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
