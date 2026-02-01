using System;
using STool.CollectionUtility;
using UnityEngine;

namespace Global
{
    public sealed class GlobalLevelMenuUI : MonoBehaviour
    {
        public LevelSlot[] levelSlots;
        public ReusableCollection<GlobalLevelMenuSlot> globalLevelMenuSlots;
        public Transform slotRoot;
        public GameObject slotPrefab;
        #region ReusableCollection
        public GlobalLevelMenuSlot GenerateSlot()
        {
            return Instantiate(slotPrefab, slotRoot).GetComponent<GlobalLevelMenuSlot>();
        }
        public void DestroySlot(GlobalLevelMenuSlot slot) => Destroy(slot.gameObject);
        public void GetSlot(GlobalLevelMenuSlot slot) => slot.gameObject.SetActive(true);
        public void PushSlot(GlobalLevelMenuSlot slot)=>slot.gameObject.SetActive(false);

        #endregion
        
        #region Mono
        private void Awake()
        {
            globalLevelMenuSlots = new(GenerateSlot,DestroySlot,GetSlot,PushSlot);
            
            _onLevelEnter = OnLevelEnter;
            _onLevelExit = OnLevelExit;
            GlobalShare.EventBus.Subscribe(_onLevelEnter);
            GlobalShare.EventBus.Subscribe(_onLevelExit);
            
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            // foreach (var slot in levelSlots)
            // {
            //     var slotUI = globalLevelMenuSlots.Get();
            //     slotUI.Init(slot.name, slot.prefab);
            // }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        private void OnDisable()
        {
            globalLevelMenuSlots.ClearToPool();
        }

        
        #endregion

        #region Event
        private Action<Global_EnterLevel> _onLevelEnter;
        private Action<Global_ExitLevel>  _onLevelExit;
        private void OnLevelEnter(Global_EnterLevel evt)
        {
            gameObject.SetActive(false);
        }

        private void OnLevelExit(Global_ExitLevel evt)
        {
            gameObject.SetActive(true);
        }
        #endregion
        

        [Serializable] public sealed class LevelSlot
        {
            public GameObject prefab;
            public string name;
        }
    }
}
