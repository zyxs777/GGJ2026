using Global;
using Rewired;
using Scene;
using Sirenix.OdinInspector;
using STool;
using STool.CollectionUtility;
using UnityEngine;

namespace Player
{
    public sealed class PlayerMotion : MonoBehaviour
        , IPhyMotion
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

            _jumpCurrent -= _jumpCurrent < 0 ? GlobalShare.GlobalTimeDelta : 0;

            rushCounter.Use(GlobalShare.GlobalTimeDelta);
        }

        #region Collision
        private readonly ContactPoint[] _contacts = new ContactPoint[32];
        private int _contactCnt;
        private void OnCollisionStay(Collision other)
        {
            var contactImp = Vector3.zero;
            _contactCnt = Mathf.Min(_contacts.Length, other.GetContacts(_contacts));
            
            for (var index = 0; index < _contactCnt; index++)
            {
                var contact = _contacts[index];
                if (Vector3.Dot(contact.normal, contact.impulse) > 0)
                {
                    contactImp += contact.impulse;
                }
            }
            _jumpCurrent = contactImp.y > 0 ? jumpRefresh : _jumpCurrent;
            _impulseVelocity += contactImp;
        }

        #endregion
        #endregion

        #region Rewired Registration
        private Rewired.Player _player;
        private void RegisterCtrl()
        {
            _player ??= Rewired.ReInput.players.GetPlayer(playerIndex);
            _player.AddInputEventDelegate(DoJump, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Jump");
            _player.AddInputEventDelegate(DoRush, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Rush");
        }

        private void UnRegisterCtrl()
        {
            if (!ReInput.isReady) return;
            _player.RemoveInputEventDelegate(DoJump, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Jump");
            _player.RemoveInputEventDelegate(DoRush, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Rush");
        }

        #endregion

        #region Motion
        [FoldoutGroup("Motion")] [SerializeField] private float inputAcceleration = 10;
        [FoldoutGroup("Motion")] [SerializeField] private float inputSpeedMaximum = 5;
        [FoldoutGroup("Motion")] [SerializeField] private float impulseDeclineValue = 5;

        [FoldoutGroup("Motion")] [SerializeField] private Vector3 gravityAcceleration = new(0, -10, 0);
        
        
        private DecoratedValue<Vector3> _motionCalculation;
        private DecoratedValue<Vector3>.ModifierCollectionToken _inputToken;
        private DecoratedValue<Vector3>.ModifierCollectionToken _impulseToken;
        [FoldoutGroup("Motion")] [ShowInInspector] [ReadOnly] private Vector3 _impulseVelocity;
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
            var dirAngle = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
            _velocityInput = _velocityInput.Rotate(dirAngle);
            
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
            _impulseVelocity += GlobalShare.GlobalTimeDelta * gravityAcceleration;  //Gravity
            
            var dir = _impulseVelocity.normalized;
            var mag = Mathf.Max(0, _impulseVelocity.magnitude - impulseDeclineValue * GlobalShare.GlobalTimeDelta);
            var impRes = mag * dir;
            
            _impulseToken.SetValue(impRes, false);
            _impulseVelocity = impRes;
        }
        #endregion
        
        private void DealVelocity()
        {
            _motionCalculation.Recompute();
            var output = _motionCalculation.Value;

            if (!rig.isKinematic)
                rig.velocity = output;
        }
        #endregion

        #region Jump
        [FoldoutGroup("Jump")] [SerializeField] private float jumpVelocity = 10;
        [FoldoutGroup("Jump")] [SerializeField] private float jumpRefresh = .2f;
        [FoldoutGroup("Jump")] [ShowInInspector] [ReadOnly] private float _jumpCurrent;
        private void DoJump(InputActionEventData data)
        {
            if (_jumpCurrent < 0) return;
            _jumpCurrent = -1;

            var jumpImp = Vector3.up;
            var contactNormal = Vector3.zero;
            for (var i = 0; i < _contactCnt; i++)
            {
                var contact = _contacts[i];
                contactNormal += contact.normal;
            }
            jumpImp = (jumpImp + contactNormal.normalized + .5f * _velocityInput.ConvertXZ().normalized).normalized;
            
            //Jump Set
            rig.transform.position += new Vector3(0, .1f, 0);
            _impulseVelocity += jumpVelocity * jumpImp;
        }
        #endregion
        
        #region Rush
        [FoldoutGroup("Rush")] [SerializeField] private float rushVelocity = 10;

        [FoldoutGroup("Rush")] [SerializeReference] [HideReferenceObjectPicker]
        private ConsumeCounterFloat rushCounter = new() { refreshTo = 2 };
        private void DoRush(InputActionEventData data)
        {
            if(!rushCounter.CanUse())return;
            rushCounter.Refresh();

            _impulseVelocity += _velocityInput.ConvertXZ().normalized * rushVelocity;
        }
        #endregion

        #region IPhyMotion
        public void AddImpulse(Vector3 impulse)
        {
            _impulseVelocity += impulse;
        }

        #endregion
    }
}
