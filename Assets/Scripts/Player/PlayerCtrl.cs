using UnityEngine;

namespace Player
{
    public class PlayerCtrl : MonoBehaviour
    {


        #region Rewired Registration
        private void RegisterCtrl()
        {
            var player = Rewired.ReInput.players.GetPlayer(0);
            // player.AddInputEventDelegate();
        }

        private void UnRegisterCtrl()
        {
            var player = Rewired.ReInput.players.GetPlayer(0);
            
        }

        #endregion
        
        
    }
}
