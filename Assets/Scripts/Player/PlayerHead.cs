using System;
using Global;
using Rewired;
using Sirenix.OdinInspector;
using STool;
using UnityEngine;

namespace Player
{
    public sealed class PlayerHead : MonoBehaviour
    {
        #region Setting

        [FoldoutGroup("Settings")] [SerializeField] private int playerIndex = 0;

        #endregion
        
        #region Mono
        
        private void OnEnable()
        {
            DoRewired();   
        }

        private void OnDisable()
        {
            DoNotRewired();
        }

        #region Update

        private void Update()
        {
            _rightInput = _player.GetAxis2D("moveX-r", "moveY-r");
            UpdateBodyAndHead(GlobalShare.GlobalTime.Value * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            
        }

        #endregion

        #endregion

        #region Control
        private Vector2 _rightInput;
        private Rewired.Player _player;
        private void DoRewired()
        {
            _player = ReInput.players?.GetPlayer(playerIndex);
        }

        private void DoNotRewired()
        {
            if (!ReInput.isReady) return;
            _player = ReInput.players?.GetPlayer(playerIndex);
        }
        #endregion

        #region Camera Control
        [FoldoutGroup("Camera Control")] [SerializeReference] private Transform rootTransform;
        [FoldoutGroup("Camera Control")] [SerializeReference] private Transform headTransform;
        [FoldoutGroup("Camera Control")] [SerializeField] private Vector3 headLocalPosition;

        [FoldoutGroup("Camera Control")] [SerializeField] private float bodyRotateSpeed = 40;
        
        [FoldoutGroup("Camera Control")] [SerializeField] private float headRotateSpeed = 20;

        [FoldoutGroup("Camera Control")] [SerializeField] private Vector2 headPitchAngle = new(-20, 20);
        
        [FoldoutGroup("Camera Control")] [SerializeField] private float breatheAmplitude = 0.1f;
        [FoldoutGroup("Camera Control")] [SerializeField] private float breathePeriod = 1f;
        private float _loopCounter;
        
        private void UpdateBodyAndHead(float deltaTime)
        {
            //Body
            var rotate = _rightInput.x;
            rotate *= bodyRotateSpeed * deltaTime;
            
            rootTransform.Rotate(Vector3.up, rotate);
            
            //Head
            var pitch = -_rightInput.y;
            pitch *= headRotateSpeed * deltaTime;
            
            var curPitch = headTransform.localRotation.eulerAngles.x;
            curPitch = curPitch > 180 ? curPitch - 360 : curPitch;
            curPitch += pitch;  
            curPitch = Mathf.Clamp(curPitch, headPitchAngle.x, headPitchAngle.y);
            
            headTransform.localRotation = Quaternion.Euler(curPitch, 0, 0);
            
            //Head Breath
            _loopCounter += deltaTime;
            _loopCounter %= breathePeriod;
            var headBreatheSampling = _loopCounter / breathePeriod;
            var amplitude = Mathf.Sin(2 * Mathf.PI * headBreatheSampling);
            amplitude *= breatheAmplitude;
            headTransform.localPosition = headLocalPosition + new Vector3(0, amplitude, 0);
        }
        
        #endregion
    }
}
