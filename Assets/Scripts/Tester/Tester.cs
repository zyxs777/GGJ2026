using Global;
using Player;
using UnityEngine;

namespace Tester
{
    public sealed class Tester : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G)) 
                SendUIActive();
            if (Input.GetKeyDown(KeyCode.H))
                GlobalShare.EventBus.Publish(new RoundPanelSelection.RoundPanelEvt_RequestSelected());
        }

        private void SendUIActive()
        {
            GlobalShare.EventBus.Publish(new RoundPanelSelection.RoundPanelEvt_Init(){Count = 8});
        }
    }
}
