using Sirenix.OdinInspector;
using STool.SBlackBoard;
using STool.SBlackBoard.Unity;
using STool.SInterfaces;
using UnityEngine;

namespace STool.SConditions.BlackboardAdapter
{
    [DisallowMultipleComponent]
    public class SBlackboardMono : MonoBehaviour
        , IValueSource
        , IBakeTarget
    {
        [HideLabel] [HorizontalGroup("0")] public SBlackboardDefinition definition;
        [SerializeReference] [HideReferenceObjectPicker] [HorizontalGroup("0")]
        private SBlackboardMono parent;
        private readonly Blackboard _blackboard = new();
        private readonly SBlackboardValueSource _valueSource = new();
        public IBlackboard GetBlackboard() => _blackboard;
        
        public bool TryGet<T>(string id, out T value) => _valueSource.TryGet(id, out value);
        public void Set<T>(string id, T value) => _valueSource.Set(id, value);
        public void Create<T>(string id, T value) => _valueSource.Create(id, value);

        private void Awake()
        {
            definition?.SetBlackBoard(_blackboard);
            _blackboard.Parent = parent?._blackboard;
            _valueSource.SetBlackboard(_blackboard);
        }
        
        //Editor Support
        public void BakeCondition(ICondition iCondition)
        {
            if (!definition) return;
            _valueSource.SetDefinition(definition);
            _valueSource.BakeCondition(iCondition);
        }
        
        #if UNITY_EDITOR
        #region Debug
        [ShowInInspector][ReadOnly][HideLabel][MultiLineProperty(10)]
        private string DebugBlackboard => _blackboard?.ToString();
        [Button] private void DebugPrint() => Debug.Log(DebugBlackboard);

        #endregion
        public void DoBake() => BakeBlackboard();
        #region Tool
        [FoldoutGroup("Tool")]
        [Button]
        public void BakeBlackboard()
        {
            //Build table of current entries
            
            //do ref cnt of components to each entry
            //comp with guid "", insert new var
            OnEachTrans(transform);
            
            //remove entry of 0 cnt
            
            Debug.Log(definition);
        }

        private void OnEachTrans(Transform trans)
        {
            var count = trans.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = trans.GetChild(i);
                
                //Stop where it is another blackboard driver
                if (child.TryGetComponent(out SBlackboardMono sbm))
                {
                    if (sbm.definition)
                    {
                        sbm.definition.Parent = definition;
                        sbm.parent = this;
                        sbm.BakeBlackboard();
                        UnityEditor.EditorUtility.SetDirty(sbm.definition);
                    }
                    continue;
                }
                
                //Recurse on each child
                OnEachTrans(child);
            }
            
            //Responding to Entries
            var comps = trans.GetComponents(typeof(ISBlackboardBakeTarget));
            if (comps == null) return;
            foreach (var comp in comps)
            {
                if (comp is not ISBlackboardBakeTarget target) continue;
                target.ValueSource = this;
                target.DoBake(this);
                UnityEditor.EditorUtility.SetDirty(comp);
            }
        }
        #endregion
        #endif
    }
}
