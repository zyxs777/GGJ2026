using System;
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
            GlobalShare.GlobalTime.OnValueChanged = OnTimeScaled;
            _timeDelta = Time.fixedDeltaTime;
        }
        private void FixedUpdate()
        {
            UpdateTime();
        }

        #region Time Management
        private float _timeDelta;
        private void OnTimeScaled(float scale)
        {
            _timeDelta = Time.fixedDeltaTime * scale;
        }

        private void UpdateTime()
        {
            Physics.Simulate(_timeDelta);
        }
        #endregion
    }
}
