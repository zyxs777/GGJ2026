using Global;
using Player;
using PrimeTween;
using Rewired;
using UnityEngine;

namespace Tester
{
    public sealed class Tester : MonoBehaviour
    {
        private void Update()
        {
            if (Input.anyKeyDown)
                RewiredRumble.OneShot(ReInput.players.GetPlayer(0), RewiredRumble.MotorSide.Left, 1, 5,
                    Ease.InOutCubic);
        }
    }
}
