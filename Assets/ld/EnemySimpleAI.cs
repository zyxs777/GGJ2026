using System;
using Global;
using Player;
using Scene;
using Sirenix.OdinInspector;
using STool.CollectionUtility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ld
{
    public sealed class EnemySimpleAI : MonoBehaviour, IPhyMotion
    {
        [Header("Refs")] 
        [SerializeReference] private Rigidbody rig;
        [SerializeField] private HelmetHitTiltPersistentShake _helmet;
        [SerializeField] private Camera _cam;

        [Header("Z Move")]
        [SerializeField] private float _moveSpeed = 3.0f;
        [SerializeField] private float _startZ = 12f;     // 出生时的远处 Z
        [SerializeField] private float _attackZ = 2.0f;   // 到这个 Z 就开始攻击
        [SerializeField] private float _stopZ = 1.6f;     // 停下来的最近 Z

        [Header("X Offset")]
        [SerializeField] private float _xOffsetRange = 3.0f; // 左右随机范围

        [Header("y Offset")]
        [SerializeField] private float _yOffsetRange = 3.0f; // 左右随机范围
        
        [Header("Attack")]
        [SerializeField] private Vector2 _attackInterval = new Vector2(0.6f, 1.4f);
        [SerializeField] private Vector2 _attackStrength = new Vector2(0.6f, 1.0f);
        [SerializeField] private float attackDamage = 6;
        [SerializeReference] private Animator animator;
        [SerializeField] private string attackAnima;
        
        private float _nextAttackTime;
        private Camera _worldCam;
        private HelmetAttackIndicator _indicator;
        Transform _target;
        private Vector3 _offset;

        private void Awake()
        {
            if (_cam == null) _cam = Camera.main;
            if (_helmet == null) _helmet = FindObjectOfType<HelmetHitTiltPersistentShake>();
            if (_worldCam == null) _worldCam = Camera.main;
            if (_indicator == null) _indicator = FindObjectOfType<HelmetAttackIndicator>();

            InitMotionDecorator();
            
            // ✅ 出生位置：只随机 X，Z 在远处
            var p = transform.position;
            p.x = Random.Range(-_xOffsetRange, _xOffsetRange);
            p.z = _startZ;
            _offset = new Vector3(p.x, p.y,0);
            
            if (!_target) _target = FindFirstObjectByType<PlayerMotion>().transform;
            
            ScheduleNextAttack();
        }

        
        private DecoratedValue<Vector3> _motionCalculation;
        private DecoratedValue<Vector3>.ModifierCollectionToken _inputToken;
        private DecoratedValue<Vector3>.ModifierCollectionToken _impulseToken;
        [FoldoutGroup("Motion")] [ShowInInspector] [ReadOnly] private Vector3 _impulseVelocity;
        [FoldoutGroup("Motion")] [SerializeField] private float impulseDeclineValue = 5;
        [FoldoutGroup("Motion")] [SerializeField] private Vector3 gravityAcceleration = new(0, -10, 0);
        private Vector3 Vec3Add(Vector3 a, Vector3 b) => a + b;
        private void InitMotionDecorator()
        {
            _motionCalculation ??= new(Vector3.zero);
            _inputToken = _motionCalculation.Add(Vector3.zero, Vec3Add);
            _impulseToken = _motionCalculation.Add(Vector3.zero, Vec3Add);
        }
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
        #region IPhyMotion
        public void AddImpulse(Vector3 impulse)
        {
            _impulseVelocity += impulse;
        }

        #endregion
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
            _impulseVelocity += contactImp;
        }

        #endregion

        private void DealVelocity()
        {
            _motionCalculation.Recompute();
            var output = _motionCalculation.Value;

            if (!rig.isKinematic)
                rig.velocity = output;
        }
        
        private void FixedUpdate()
        {
            var p = transform.position;
            var distance = Vector3.Distance(p, _target.position+_offset);
            var direction = (_target.position + _offset - p).normalized;

            var canMove = distance > _stopZ;
            _inputToken.SetValue( canMove ? direction * _moveSpeed : Vector3.zero);
            DoImpulseDecline();
            DealVelocity();
        }

        
        private void Update()
        {
            var p = transform.position;
            var distance = Vector3.Distance(p, _target.position+_offset);
            var direction = (_target.position + _offset - p).normalized;
            
            // 2️⃣ 到攻击区间后随机攻击
            if (distance <= _attackZ && Time.time >= _nextAttackTime)
            {
                DoAttack();
                ScheduleNextAttack();
            }
        }

        private void DoAttack()
        {
            if (_helmet == null || _cam == null) return;

            // ✅ 敌人相对“头盔中心”的位置 = 屏幕投影
            Vector2 screenPos = _cam.WorldToScreenPoint(transform.position);
            float strength = Random.Range(_attackStrength.x, _attackStrength.y);

            animator.Play(attackAnima);
            
            _helmet.ApplyHitScreenPos(
                new HelmetHitTiltPersistentShake.AttackPack()
                {
                    HitScreenPos = screenPos,
                    Strength01 = strength,
                    BaseHit = attackDamage,
                    AttackerPos = transform.position
                });
        
            _indicator?.ShowAtWorldPos(transform.position,_cam);
        }

        private void ScheduleNextAttack()
        {
            _nextAttackTime = Time.time + Random.Range(_attackInterval.x, _attackInterval.y);
        }
    }
}
