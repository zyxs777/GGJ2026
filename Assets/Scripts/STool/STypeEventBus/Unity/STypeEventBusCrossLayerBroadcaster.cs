using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool.SInterfaces;
using UnityEngine;

namespace STool.STypeEventBus.Unity
{
    /// <summary>
    /// 基于Transform层级向上广播消息
    /// </summary>
    public sealed class STypeEventBusCrossLayerBroadcaster : SerializedMonoBehaviour
        , ITypeBusSubscribeRegister
        , IBakeTarget
    {
        #region TypeEventBusSubscriber
        ITypeBusSubscriber ITypeBusSubscribeRegister.Subscriber => _subscriber;
        private ITypeBusSubscriber _subscriber;

        public void Init(ITypeBusSubscriber subscriber)
        {
            _subscriber = subscriber;
            InitChannels();
        }

        #endregion

        #region Evt Cross Layer Deal

        [OdinSerialize] [NonSerialized] [HideReferenceObjectPicker]
        public List<EvtChannel> EvtChannels = new();

        private void InitChannels()
        {
            for (var i = 0; i < EvtChannels.Count; i++)
            {
                var channel = EvtChannels[i];
                channel.Init(_subscriber);
            }
        }
        #endregion
        
        #region Evt Cross Layer Def
        [Serializable] public sealed class EvtChannel : ITypeBusPublisher
        {
            [HorizontalGroup] public ISTypeCrossEvt CrossEvt; 
            [HorizontalGroup] public int direction = 1;
            public List<ITypeBusPublisher> Publishers = new();

            public void Init(ITypeBusSubscriber subscriber)
            {
                CrossEvt.DoSubscribe(subscriber);
                CrossEvt.Publisher = this;
            }
            public void Publish<T>(T message)
            {
                // Debug.Log($"Publishing {message}");
                foreach (var publisher in Publishers) publisher.Publish(message);
            }
        }
        public interface ISTypeCrossEvt
        {
            public void DoSubscribe(ITypeBusSubscriber subscriber);
            public ITypeBusPublisher Publisher { get; set; }
        }
        public interface ISTypeCrossEvtQuickImplement<in T> where T : struct
        {
            public ITypeBusPublisher Publisher { get; set; }
            public void ChannelAct(T evt) { Publisher.Publish(evt); }
        }
        #endregion

        #region Bake
        #if UNITY_EDITOR
        public void DoBake()
        {
            foreach (var ebd in EvtChannels)
                BakeOneChannel(ebd);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        #region Baking
        private int _bakeDirection;
        private void BakeOneChannel(EvtChannel channel)
        {
            channel.Publishers.Clear();
            _bakeDirection = channel.direction;
            switch (_bakeDirection)
            {
                case 0: return;
                case > 0:   //往子烘焙
                    var childCnt = transform.childCount;
                    for (var i = 0; i < childCnt; i++)
                        BakeDownLayer(channel, transform.GetChild(i), 1);
                    break;
                case < 0:   //往父烘焙
                    if (!transform.parent) return;
                    BakeUpLayer(channel, transform.parent, -1);
                    break;
            }
        }

        private void BakeUpLayer(EvtChannel channel, Transform trans, int curDir)
        {
            if (curDir < _bakeDirection) return;
            
            var publishers = trans.GetComponents<ITypeBusPublisher>();
            channel.Publishers.AddRange(publishers);

            if (!trans.parent) return;
            curDir--;
            BakeUpLayer(channel, trans.parent, curDir);
        }
        private void BakeDownLayer(EvtChannel channel,Transform trans, int curDir)
        {
            if (curDir > _bakeDirection) return;
            
            var publishers = trans.GetComponents<ITypeBusPublisher>();
            channel.Publishers.AddRange(publishers);

            var childCnt = trans.childCount;
            if (childCnt == 0) return;
            curDir++;
            for (var i = 0; i < childCnt; i++)
                BakeDownLayer(channel, trans.GetChild(i), curDir);
        }
        #endregion
        #endif
        #endregion

        
    }
}
