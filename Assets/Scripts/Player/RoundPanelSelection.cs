using System;
using System.Collections.Generic;
using Global;
using Rewired;
using Sirenix.OdinInspector;
using STool.CollectionUtility;
using UnityEngine;

namespace Player
{
    public sealed class RoundPanelSelection : MonoBehaviour
    {
        #region Mono
        private void Awake()
        {
            _roundSelectPool ??= new ReusableCollection<GameObject>(CreateSlot, DestroySlot, DoActivateSlot, DoDeactivateSlot);
            DoListenGlobal();
        }   
        private void OnDestroy()
        {
            DoNotListenGlobal();

        }

        private void OnEnable()
        {
            RegisterCtrl();
            GlobalShare.CenterCursor();
        }
        private void OnDisable()
        {
            UnregisterCtrl();
            _roundSelectPool.ClearToPool();
        }
     

        private void Update()
        {
            UpdateRoundPanel();
        }

        #endregion

        #region Input
        private Rewired.Player _player;
        private void RegisterCtrl()
        {
            _player ??= ReInput.players.GetPlayer(0);
            
        }


        private void UnregisterCtrl()
        {
            _player ??= ReInput.players.GetPlayer(0);
            
        }
        #endregion

        #region Listen

        private void DoListenGlobal()
        {
            _roundPanelInit ??= RoundPanelInit;
            _roundPanelRequestSelected ??= RoundPanelRequestSelected;
            GlobalShare.EventBus.Subscribe(_roundPanelInit);
            GlobalShare.EventBus.Subscribe(_roundPanelRequestSelected);
        }
        private void DoNotListenGlobal()
        {
            GlobalShare.EventBus.Unsubscribe(_roundPanelInit);
            GlobalShare.EventBus.Unsubscribe(_roundPanelRequestSelected);
        }
        
        private Action<RoundPanelEvt_Init> _roundPanelInit;
        private Action<RoundPanelEvt_RequestSelected> _roundPanelRequestSelected;
        private int _count;
        private void RoundPanelInit(RoundPanelEvt_Init evt)
        {
            _roundSelectPool.ClearToPool();
            _activeSelectPool.Clear();
            
            gameObject.SetActive(true);
            var angle = 360f / evt.Count;
            _count = evt.Count;
            
            for (var i = 0; i < _count; i++)
            {
                var rotate = angle * i;
                var slot = _roundSelectPool.Get();
                var trans =  slot.transform;
                trans.localPosition = selectionSlotRadius * (Quaternion.Euler(0, 0, rotate) * Vector3.up);
                
                _activeSelectPool.Add(slot);
            }
            
            selectionCursor.SetAsLastSibling();
        }
        private void RoundPanelRequestSelected(RoundPanelEvt_RequestSelected evt)
        {
            var selected = CalculateSelected();
            GlobalShare.EventBus.Publish(new RoundPanelEvt_Selection() { SelectedIndex = selected});
        }

        #region Events
        public struct RoundPanelEvt_Init
        {
            public int Count;
        }
        public struct RoundPanelEvt_RequestSelected { }
        public struct RoundPanelEvt_Selection
        {
            public int SelectedIndex;
        }
        

        #endregion
        #endregion
        
        #region RoundInput

        [FoldoutGroup("Input")] [ShowInInspector]
        private Vector2 _rightInput = Vector2.zero;

        [FoldoutGroup("Input")] [SerializeField]
        private float mouseWeight = .1f;
        private void UpdateRoundPanel()
        {
            if (_player?.controllers.GetLastActiveController() == null) return;
            var right = _player.GetAxis2D("moveX-r", "moveY-r");
            if (_player.controllers.GetLastActiveController().type == ControllerType.Joystick)
            {
                _rightInput = right;
            }
            else
            {
                _rightInput += right.normalized * mouseWeight;
            }
            _rightInput = Vector2.ClampMagnitude(_rightInput, 1);
            UpdateCursorPos(_rightInput);
        }

        private int CalculateSelected()
        {
            if (_count == 0) return -1;
            var doneDir = Vector2.ClampMagnitude(_rightInput, 1);

            if (_rightInput.sqrMagnitude < .5f) return -1;
            
            var angle = 360f / _count;
            var rotate = Vector2.SignedAngle(Vector2.up, doneDir);
            rotate += rotate < 0 ? 360 : 0;
            rotate += angle / 2f;

            var selected = Mathf.FloorToInt(rotate / angle);
            return selected;
        }
        #endregion

        #region Selection List

        [FoldoutGroup("SelectionList")] [SerializeReference]
        private GameObject selectPrefab;

        private ReusableCollection<GameObject> _roundSelectPool;
        private readonly List<GameObject> _activeSelectPool = new();
        #region SlotPool Methods
        private GameObject CreateSlot()
        {
            return Instantiate(selectPrefab, transform);
        }
        private void DoActivateSlot(GameObject slot)
        {
            slot.SetActive(true);
        }
        private void DoDeactivateSlot(GameObject slot)
        {
            slot.SetActive(false);
        }
        private void DestroySlot(GameObject slot)
        {
            Destroy(slot);
        }
        #endregion

        #endregion
        
        #region Selection Show

        [FoldoutGroup("SelectionCursor")] [SerializeReference]
        private Transform selectionCursor;

        [FoldoutGroup("SelectionCursor")] [SerializeField]
        private float selectionRadius = 500;

        [FoldoutGroup("SelectionCursor")] [SerializeField]
        private float selectionSlotRadius = 450;

        private void UpdateCursorPos(Vector2 direction)
        {
            selectionCursor.localPosition = direction * selectionRadius;
            var selected = CalculateSelected();
            
            for (var i = 0; i < _activeSelectPool.Count; i++)
            {
                var slot = _activeSelectPool[i];
                slot.transform.localScale = (i == selected ? 2 : 1) * Vector3.one;
            }
        }
        #endregion
    }
}
