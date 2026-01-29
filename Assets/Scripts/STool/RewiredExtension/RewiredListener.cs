using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.RewiredExtension
{
    public class RewiredListener : MonoBehaviour
    {
        [SerializeField] private Mode mode;
        [SerializeField] private int playerId;

        [SerializeReference, HideReferenceObjectPicker]
        private List<ListerSlot> slots;
        public UnityEngine.Events.UnityEvent onListen;
        private void OnEnable()
        {
            if (mode == Mode.Auto) RegisterListening();
        }
        private void OnDisable()
        {
            if (mode == Mode.Auto) UnregisterListening();
        }

        public void RegisterListening()
        {
            if (!ReInput.isReady) return;
            var player = ReInput.players.GetPlayer(playerId);
            foreach (var slot in slots)
            {
                player.AddInputEventDelegate(OnListen, UpdateLoopType.Update, slot.eventType, slot.actionName);    
            }
        }
        public void UnregisterListening()
        {
            if (!ReInput.isReady) return;
            var player = ReInput.players.GetPlayer(playerId);
            foreach (var slot in slots)
            {
                player.RemoveInputEventDelegate(OnListen, UpdateLoopType.Update, slot.eventType, slot.actionName);    
            }
        }

        private void OnListen(InputActionEventData data)
        {
            onListen.Invoke();
        }
        
        
        private enum Mode
        {
            Auto,
            Manual
        }
        [System.Serializable] public class ListerSlot
        {
            [HorizontalGroup("0")] 
            public InputActionEventType eventType = InputActionEventType.ButtonJustPressed;

            [HorizontalGroup("0"), ValueDropdown(RewiredEditorUtility.GetActionName)]
            public string actionName;
        }
    }
}
