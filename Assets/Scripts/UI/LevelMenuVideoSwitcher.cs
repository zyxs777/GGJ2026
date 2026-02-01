using System;
using Global;
using UnityEngine;
using UnityEngine.Video;

namespace UI
{
    public sealed class LevelMenuVideoSwitcher : MonoBehaviour
    {
        [SerializeReference] private VideoPlayer videoPlayer;
        [SerializeReference] private VideoClip idle;
        [SerializeReference] private VideoClip enter;

        private void OnEnable()
        {
            videoPlayer.clip = idle;
            videoPlayer.Play();

            _onEnter ??= OnEnter;
            GlobalShare.EventBus.Subscribe(_onEnter);
        }

        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onEnter);
        }

        private Action<GlobalLerpUI.UIEvtLerp> _onEnter;
        private void OnEnter(GlobalLerpUI.UIEvtLerp evt)
        {
            videoPlayer.clip = enter;
            videoPlayer.Play();
        }
    }
}
