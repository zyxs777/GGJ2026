using System;
using PrimeTween;
using STool.CollectionUtility;
using UnityEngine;

namespace Global
{
    public sealed class GameMgr : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GlobalShare.Reset();

        }

        
        private void Awake()
        {
            Physics.simulationMode = SimulationMode.Script;

            _globalPause = GlobalShare.GlobalTime.Add(1, (f, f1) => f * f1);
            _globalPauseCallback ??= GlobalPause;
            _globalResumeCallback ??= GlobalResume;
            GlobalShare.EventBus.Subscribe(_globalPauseCallback);
            GlobalShare.EventBus.Subscribe(_globalResumeCallback);
            
            //临时处理
            Tween.Delay(.1f, () => { GlobalShare.EventBus.Publish(new Global_ExitLevel()); });
        }
        private void FixedUpdate()
        {
            UpdateTime();
        }

        #region Time Management

        #region Time Scale
        private void UpdateTime()
        {
            if (GlobalShare.GlobalTimeDelta > 0)
                Physics.Simulate(GlobalShare.GlobalTimeDelta);
        }
        #endregion

        #region Time Pause

        private DecoratedValue<float>.ModifierCollectionToken _globalPause;
        private Action<Global_GamePause> _globalPauseCallback;
        private Action<Global_GameResume> _globalResumeCallback;

        private void GlobalPause(Global_GamePause evt)
        {
            _globalPause.SetValue(0);
        }

        private void GlobalResume(Global_GameResume evt)
        {
            _globalPause.SetValue(1);
        }
        #endregion

        #endregion
    }
}
