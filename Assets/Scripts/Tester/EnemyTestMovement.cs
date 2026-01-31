using System;
using Global;
using Player;
using UnityEngine;

namespace Tester
{
    public class EnemyTestMovement : MonoBehaviour
    {
        private GameObject _player;
        private PlayerIdentification.PlayerIdentificationData _data;
        private Action<PlayerIdentification.PlayerIdentificationData> _getPlayerIdentificationData;
        private void GetPlayerInfo(PlayerIdentification.PlayerIdentificationData data) => _data = data;
        
        
        
        private void OnEnable()
        {
            _getPlayerIdentificationData ??= GetPlayerInfo;
            GlobalShare.EventBus.Subscribe(_getPlayerIdentificationData);
            GlobalShare.EventBus.Publish(new PlayerIdentification.PlayerIDRequest());
        }
        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_getPlayerIdentificationData);
        }

        private void FixedUpdate()
        {
            var dir = -transform.position + _data.PlayerGameObject.transform.position;
            var displacement = dir.normalized * GlobalShare.GlobalTimeDelta;
            transform.position += displacement;
        }
    }
}
