using System;
using Global;
using Rewired;
using UnityEngine;

namespace Player
{
    public sealed class PlayerGlobalCall : MonoBehaviour
    {
        #region Mono
        private Rewired.Player _player;

        private void Awake()
        {
            _gameEsc = () => GlobalShare.EventBus.Publish(new Global_GamePause());
            _gamePause = OnGamePause;
            _gameResume = OnGameResume;
            GlobalShare.EventBus.Subscribe(_gamePause);
            GlobalShare.EventBus.Subscribe(_gameResume);
        }
        private void OnEnable()
        {
            _player = ReInput.players.GetPlayer(0);
            _player.AddInputEventDelegate(OnEsc, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Esc");
        }
        private void OnDisable()
        {
            if (!ReInput.isReady) return;
            _player.RemoveInputEventDelegate(OnEsc, UpdateLoopType.Update,InputActionEventType.ButtonJustPressed, "Esc");
        }

        private void OnEsc(InputActionEventData data)
        {
            _gameEsc?.Invoke();
        }
        #endregion

        #region System Call
        private Action<Global_GamePause> _gamePause;
        private Action<Global_GameResume> _gameResume;
        private Action _gameEsc;
        private void OnGamePause(Global_GamePause pause)
        {
            _gameEsc = () => GlobalShare.EventBus.Publish(new Global_GameResume());
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnGameResume(Global_GameResume resume)
        {
            _gameEsc = () => GlobalShare.EventBus.Publish(new Global_GamePause());
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endregion
    }
}
