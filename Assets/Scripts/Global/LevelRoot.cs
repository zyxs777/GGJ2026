using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace Global
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class LevelRoot : MonoBehaviour
    {
        [SerializeReference] private PlayableDirector director;
        private void OnEnable()
        {
            _onLevelExit ??= OnLevelExit;
            _onRecEnemyReg ??= OnRecEnemyReg;
            _onRecEnemyUnReg ??= OnRecEnemyUnReg;
            GlobalShare.EventBus.Subscribe(_onLevelExit);
            GlobalShare.EventBus.Subscribe(_onRecEnemyReg);
            GlobalShare.EventBus.Subscribe(_onRecEnemyUnReg);
            GlobalShare.EventBus.Publish(new Global_EnterLevel());

            _onLevelEnd ??= OnDirectorEnd;
            director.stopped += _onLevelEnd;
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onLevelExit);
            GlobalShare.EventBus.Unsubscribe(_onRecEnemyReg);
            GlobalShare.EventBus.Unsubscribe(_onRecEnemyUnReg);
            
            director.stopped -= _onLevelEnd;
        }

        #region Level Exit
        private Action<Global_ExitLevel> _onLevelExit;
        private void OnLevelExit(Global_ExitLevel evt)
        {
            Destroy(gameObject);
        }
        #endregion

        #region Level Management
        private Action<PlayableDirector> _onLevelEnd;
        private void OnDirectorEnd(PlayableDirector playableDirector)
        {
            TryLevelExit();
        }

        private void TryLevelExit()
        {
            if (director.state == PlayState.Playing) return;
            if (_enemyPool.Count > 0) return;

            GlobalShare.EventBus.Publish(new GlobalLerpUI.UIEvtLerp()
            {
                OnLerpMiddle = () => GlobalShare.EventBus.Publish(new Global_ExitLevel())
            });
        }
        
        [ShowInInspector] private HashSet<GameObject> _enemyPool = new();
        private Action<LevelEvt_RegisterEnemy> _onRecEnemyReg;
        private Action<LevelEvt_UnregisterEnemy> _onRecEnemyUnReg;
        private void OnRecEnemyReg(LevelEvt_RegisterEnemy evt)
        {
            _enemyPool.Add(evt.Enemy);
        }
        private void OnRecEnemyUnReg(LevelEvt_UnregisterEnemy evt)
        {
            _enemyPool.Remove(evt.Enemy);
            TryLevelExit();
        }
        public struct LevelEvt_RegisterEnemy
        {
            public GameObject Enemy;
        }
        public struct LevelEvt_UnregisterEnemy
        {
            public GameObject Enemy;
        }
        #endregion

        #region Editor
        #if UNITY_EDITOR
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if(!director)
            {
                director = GetComponent<PlayableDirector>();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        
        #endif
        #endregion
    }
}
