using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.SInterfaces
{
    public class SUpdaterMono : SerializedMonoBehaviour, IBakeTarget
    {
        [SerializeField] private ITimeSource _timeSource;
        [SerializeField] private List<ITimeUpdater> _updaters = new();
        [SerializeField] private List<ITimeScaler> _scalers = new();
        [SerializeField] private List<IActivator>  _activators = new();
        private void FixedUpdate()
        {
            var delta = _timeSource.TimeDelta;
            var scale = _timeSource.TimeScale;
            
            foreach (var updater in _updaters)
                updater?.TimeUpdate(delta);
            foreach (var scaler in _scalers)
                scaler?.SetTimeScale(scale);
        }

        private void OnEnable()
        {
            foreach (var activator in _activators)
                activator?.DoOnEnable();
        }

        private void OnDisable()
        {
            foreach (var activator in _activators)
                activator?.DoOnDisable();
        }

#if UNITY_EDITOR
        public void DoBake() => BakeUpdates();
        [FoldoutGroup("Tool")]
        [Button] private void BakeUpdates()
        {
            _updaters.Clear();
            var iTimeUpdaters = transform.GetComponentsInChildren<ITimeUpdater>(true);
            foreach (var comp in iTimeUpdaters)
                _updaters.Add(comp);
            
            _scalers.Clear();
            var iScalers = transform.GetComponentsInChildren<ITimeScaler>(true);
            foreach (var comp in iScalers)
                _scalers.Add(comp);
            
            _activators.Clear();
            var iActivators = transform.GetComponentsInChildren<IActivator>(true);
            foreach (var comp in iActivators)
                _activators.Add(comp);
            
            UnityEditor.EditorUtility.SetDirty(this); 
        }
#endif
    }
}
