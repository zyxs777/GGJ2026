using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace STool.SInterfaces
{
    public class ActivatorMono : MonoBehaviour
    {
        [HorizontalGroup("Settings")] [SerializeField]
        private bool doStart = false;
        [HorizontalGroup("Settings")] [SerializeField]
        private bool doEnable = false;
        [HorizontalGroup("Settings")] [SerializeField]
        private bool doDisable = false;
        [HorizontalGroup("Settings")] [SerializeField]
        private bool doDestroy = false;

        
        [FoldoutGroup("OnStart")] [SerializeReference][ShowIf("doStart")]
        [HideReferenceObjectPicker]
        private UnityEvent onStart = new();
        
        [FoldoutGroup("OnEnable")] [SerializeReference][ShowIf("doEnable")]
        [HideReferenceObjectPicker]
        private UnityEvent onEnable = new();
        
        [FoldoutGroup("OnDisable")] [SerializeReference][ShowIf("doDisable")]
        [HideReferenceObjectPicker]
        private UnityEvent onDisable = new();

        [FoldoutGroup("OnDestroy")] [SerializeReference][ShowIf("doDestroy")]
        [HideReferenceObjectPicker]
        private UnityEvent onDestroy = new();
        
        private void Start() => onStart?.Invoke();
        private void OnEnable() => onEnable?.Invoke();
        private void OnDisable() => onDisable?.Invoke();
        private void OnDestroy() => onDestroy?.Invoke();
    }
}
