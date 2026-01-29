using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SInterfaces;
using UnityEngine;

namespace STool.STypeEventBus.Unity
{
    [DisallowMultipleComponent]
    public class STypeEventBusMono : SerializedMonoBehaviour
        , ITypeBusSubscriber
        , ITypeBusPublisher
        , IBakeTarget
    {
        public TypeEventBus EventBus => _typeEventBus;
        private readonly TypeEventBus _typeEventBus = new();
        private readonly CompositeDisposable _disposable = new();
        
        [OdinSerialize] private List<ITypeBusSubscribeRegister> _registers = new();
        [OdinSerialize] private List<ITypeBusPublishRegister> _publishers = new();
        
        #region Mono
        private void Awake()
        {
            InitEventBus();
        }
        private void OnDestroy()
        {
            _disposable.Dispose();   
        }
        #endregion

        #region Calls
        public void InitEventBus()
        {
            _typeEventBus.Reset();
            foreach (var register in _registers)
                register.Init(this);
            foreach (var publisher in _publishers)
                publisher.Init(this);
        }
        

        #endregion
        
        #region Type Subscriber
        public void Publish<T>(T message)
        {
            _typeEventBus.Publish(message);
        }
        public IDisposable Subscribe<T>(Action<T> handler)
        {
            var disposable = _typeEventBus.Subscribe(handler);
            _disposable.Add<T>(disposable);
            return disposable;
        }
        public void Unsubscribe<T>(Action<T> handler)
        {
            _typeEventBus.Unsubscribe(handler);
        }
        #endregion
        
        
        #if UNITY_EDITOR
        public void DoBake() => BakeSubscribers();
        [FoldoutGroup("Tool")] [Button]
        public void BakeSubscribers()
        {
            _registers.Clear();
            _publishers.Clear();
            OnEachTrans(transform);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        private void OnEachTrans(Transform trans)
        {
            if (trans.TryGetComponent(out STypeEventBusMono ste) && ste != this)
            {
                ste.BakeSubscribers();
                return;
            }

            var comps = trans.GetComponents(typeof(ITypeBusSubscribeRegister));
            foreach (var comp in comps)
            {
                if (comp is ITypeBusSubscribeRegister ibr)
                {
                    _registers.Add(ibr);
                }
            }
            comps =  trans.GetComponents(typeof(ITypeBusPublishRegister));
            foreach (var comp in comps)
            {
                if (comp is ITypeBusPublishRegister ipr)
                {
                    _publishers.Add(ipr);
                }
            }
            
            for (var i = 0; i < trans.childCount; i++)
                OnEachTrans(trans.GetChild(i));
        }
        
        #endif
        
    }
}
