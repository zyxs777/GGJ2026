using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public sealed class ButtonSpring : MonoBehaviour, IPointerEnterHandler
    {
        public float duration = .3f;
        public float amplitude = 1f;
        private Tween _tween;
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tween.isAlive) return;
            _tween = Tween.ShakeLocalRotation(transform, new Vector3(0, 0, amplitude), duration);
        }
    }
}
