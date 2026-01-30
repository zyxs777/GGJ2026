using System;
using Global;
using Sirenix.OdinInspector;
using STool;
using STool.CollectionUtility;
using UnityEngine;

namespace Player
{
    public sealed class PlayerMotion : MonoBehaviour
    {
        #region Player Setting

        [FoldoutGroup("PlayerSetting")] [SerializeField]
        private int playerIndex = 0;

        #endregion
        
        #region Mono
        [FoldoutGroup("Components")] [SerializeReference] private Rigidbody rig;

        private void Awake()
        {
            InitMotionDecorator();
        }

        private void OnEnable()
        {
            RegisterCtrl();
        }

        private void OnDisable()
        {
            UnRegisterCtrl();
        }

        private void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            DoImpulseDecline();
            DealVelocity();
        }

        #region Collision

        private void OnCollisionEnter(Collision other)
        {
            _impulseVelocity += other.impulse;
        }
        #endregion
        #endregion

        #region Rewired Registration
        private Rewired.Player _player;
        private void RegisterCtrl()
        {
            _player ??= Rewired.ReInput.players.GetPlayer(playerIndex);
            // player.AddInputEventDelegate();
        }

        private void UnRegisterCtrl()
        {
            _player ??= Rewired.ReInput.players.GetPlayer(playerIndex);
        }

        #endregion

        #region Motion
        [FoldoutGroup("Motion")] [SerializeField] private float inputAcceleration = 10;
        [FoldoutGroup("Motion")] [SerializeField] private float inputSpeedMaximum = 5;
        [FoldoutGroup("Motion")] [SerializeField] private float impulseDeclineValue = 5;
        [FoldoutGroup("Motion")] [SerializeField] private float jumpVelocity = 10;
        
        private DecoratedValue<Vector3> _motionCalculation;
        private DecoratedValue<Vector3>.ModifierCollectionToken _inputToken;
        private DecoratedValue<Vector3>.ModifierCollectionToken _impulseToken;
        private Vector3 _impulseVelocity;
        private Vector3 Vec3Add(Vector3 a, Vector3 b) => a + b;
        private void InitMotionDecorator()
        {
            _motionCalculation ??= new(Vector3.zero);
            _inputToken = _motionCalculation.Add(Vector3.zero, Vec3Add);
            _impulseToken = _motionCalculation.Add(Vector3.zero, Vec3Add);
        }

        #region Proactive
        [FoldoutGroup("Motion")] [ShowInInspector] [ReadOnly] private Vector2 _velocityInput;
        [FoldoutGroup("Motion")] [ShowInInspector] [ReadOnly] private Vector2 _velocityProactive;
        private void GetInput()
        {
            _velocityInput = Vector2.ClampMagnitude(_player.GetAxis2D("moveX", "moveY"), 1);
            var velInputChange = _velocityInput - _velocityProactive;
            velInputChange = Vector2.ClampMagnitude(velInputChange, GlobalShare.GlobalTimeDelta);
            _velocityProactive += velInputChange;
            //主动速度加减速
            _inputToken.SetValue( inputSpeedMaximum * _velocityProactive.ConvertXZ(), false);
        }

        #endregion

        #region Impulse
        private void DoImpulseDecline()
        {
            var dir = _impulseVelocity.normalized;
            var mag = Mathf.Max(0, dir.magnitude - impulseDeclineValue * GlobalShare.GlobalTimeDelta);
            var impRes = mag * dir;

            _impulseToken.SetValue(impRes, false);
            _impulseVelocity = impRes;
        }
        #endregion
        
        private void DealVelocity()
        {
            _motionCalculation.Recompute();
            var output = _motionCalculation.Value;
         
            rig.velocity = output;
            // var outputXZ = new Vector3(output.x, 0, output.z);
            // var velOfRig = rig.velocity;
            
            // velOfRig.x =  outputXZ.x;
            // velOfRig.z = outputXZ.z;
            // rig.velocity = velOfRig;
        }
        #endregion
    }
}
