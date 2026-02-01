using System;
using Global;
using PrimeTween;
using UnityEngine;

namespace UI
{
    public sealed class OnStartScale : MonoBehaviour
    {
        [SerializeReference] private Transform root;
        private void OnEnable()
        {
            root.localScale = Vector3.one;
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
            Tween.Scale(root, 3f, 3f, Ease.Linear);
        }
    }
}
