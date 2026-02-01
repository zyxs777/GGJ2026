using System;
using PrimeTween;
using Sirenix.OdinInspector;
using STool.CollectionUtility;
using UnityEngine;
using UnityEngine.UI;

namespace Global
{
    public sealed class GlobalLerpUI : MonoBehaviour
    {
        #region Init
        private DecoratedValue<float>.ModifierCollectionToken _token;
        private void Awake()
        {
            _token = GlobalShare.GlobalTime.Add(1, (f, f1) => f * f1);

            _onRecCmd ??= OnRecCmd;
            GlobalShare.EventBus.Subscribe(_onRecCmd);
            
            gameObject.SetActive(false);
        }

        #endregion

        #region Mono

        private float _lerpTime;
        private void OnEnable()
        {
            _token.SetValue(0);
            Tween.Alpha(image, 0, 1, _lerpTime, Ease.InOutCubic)
                .OnComplete(() => {
                        _onMid?.Invoke();
                        _token.SetValue(1);
                        Tween.Alpha(image, 1, 0, 1, Ease.InOutCubic)
                            .OnComplete(() => {   
                                    gameObject.SetActive(false);
                                });
                    });
        }
        
        #endregion

        #region UI

        [FoldoutGroup("UI")] [SerializeReference] private Image image;
        

        #endregion
        
        #region Call

        private Action _onMid;
        private Action<UIEvtLerp> _onRecCmd;
        private void OnRecCmd(UIEvtLerp evt)
        {
            _onMid = evt.OnLerpMiddle;
            _lerpTime = evt.EnterTime == 0 ? 1 : evt.EnterTime;
            gameObject.SetActive(true);    
        }
        #region Evt
        public struct UIEvtLerp
        {
            public float EnterTime;
            public Action OnLerpMiddle;
        }
        #endregion
        #endregion
        
    }
}
