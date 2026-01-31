using System;
using Global;
using PrimeTween;
using STool;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public sealed class HelmetHitEffect3D : MonoBehaviour
    {
        [SerializeReference] private GameObject effect;
        [SerializeReference] private Transform effectRoot;
        [SerializeReference] private Transform playerRoot;
        [SerializeField] private float eftRadius = 400;

        private void OnEnable()
        {
            _onHit ??= OnHit;
            GlobalShare.EventBus.Subscribe(_onHit);
        }
        private void OnDisable()
        {
            GlobalShare.EventBus.Unsubscribe(_onHit);   
        }

        private Action<HelmetHitEffectData> _onHit;
        private void OnHit(HelmetHitEffectData evt)
        {
            var pos = evt.AttackerPos;
            var dir = pos - playerRoot.position;
            var p2D = playerRoot.forward.ConvertXZ();
            var h2D = dir.normalized.ConvertXZ();
            var rotate = -Vector2.SignedAngle(p2D, h2D);

            var eftPos = eftRadius * Vector2.up.Rotate(rotate);
            var eft = Instantiate(effect, effectRoot);
            eft.transform.localPosition = eftPos;
            eft.transform.localRotation = Quaternion.Euler(0, 0, 90 -rotate);
            var eftImg = eft.GetComponent<Image>();
            Tween.Alpha(eftImg, 1, 0, 1, Ease.InBounce)
                .OnComplete(() => Destroy(eft));
        }
        public struct HelmetHitEffectData
        {
            public Vector3 AttackerPos;
        }
    }
}
