using System;
using Sirenix.OdinInspector;
using STool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace Scene
{
    [RequireComponent(typeof(SignalReceiver))]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeReference] private GameObject enemyPrefab;

        public void GenerateEnemy()
        {
            var enemy = Instantiate(enemyPrefab, transform);
            enemy.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        
        #region Editor
        #if UNITY_EDITOR
        [HideInInspector][SerializeReference] private SignalReceiver signalReceiver;
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (signalReceiver) return;
            signalReceiver = GetComponent<SignalReceiver>();
            if (!signalReceiver) return;
            
            var signal = MAssetTool.FindAllAssetsOfType<SignalAsset>().Find((asset => asset.name.Equals("EnemySpawn")));
            var unityEvt = new UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvt, GenerateEnemy);
            signalReceiver.AddReaction(signal, unityEvt);
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(signalReceiver);
        }
        [FoldoutGroup("Editor")] [SerializeField] private Color editorColor = Color.red;
        private void OnDrawGizmos()
        {
            Gizmos.color = editorColor;
            var pos0 = transform.position;
            var pos1 = transform.position + Vector3.up;
            Gizmos.DrawSphere(pos0, 0.1f);
            Gizmos.DrawLine(pos0,  pos1);
            Gizmos.DrawSphere(pos1, .2f);
            
            UnityEditor.Handles.Label(pos0, gameObject.name);
        }
        #endif
        #endregion
    }
}
