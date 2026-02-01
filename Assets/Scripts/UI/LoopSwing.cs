using System;
using PrimeTween;
using UnityEngine;

namespace UI
{
    public sealed class LoopSwing : MonoBehaviour
    {
        private void OnEnable()
        {
            Tween.ShakeScale(transform, .5f * Vector3.one, 2).SetRemainingCycles(99999);
        }
    }
}
