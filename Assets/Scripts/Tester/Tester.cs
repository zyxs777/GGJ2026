using Global;
using Player;
using PrimeTween;
using Rewired;
using UnityEngine;

namespace Tester
{
    public sealed class Tester : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                Tween.Color(spriteRenderer, Color.red, Color.white, .3f, Ease.InBounce);
            }
                
        }
    }
}
