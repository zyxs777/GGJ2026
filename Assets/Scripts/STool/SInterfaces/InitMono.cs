using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace STool.SInterfaces
{
    public class InitMono : SerializedMonoBehaviour, IBakeTarget
    {
        [SerializeField] private readonly List<IInit> _initTargets = new();
        [SerializeReference] private Transform bakeRoot;
        protected void Initialize()
        {
            foreach (var comp in _initTargets)
            {
                comp.Initialize();
            }
        }
        
        #if UNITY_EDITOR
        public void DoBake() => BakeInits();
        [FoldoutGroup("Tool")] [Button]
        private void BakeInits()
        {
            if (!bakeRoot) return;
            _initTargets.Clear();
            var iTimeUpdaters = bakeRoot.GetComponentsInChildren<IInit>(true);
            foreach (var comp in iTimeUpdaters)
            {
                _initTargets.Add(comp);
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Bake Done!");
        }
        #endif
    }
}
