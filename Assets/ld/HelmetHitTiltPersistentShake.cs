using Global;
using Player;
using STool;
using UnityEngine;

public class HelmetHitTiltPersistentShake : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform _rotRoot;      // HelmMotionRot（通常就是自己）
    [SerializeField] private RectTransform _centerRef;    // HelmFrame / UIHelm（用于取“头盔中心”）

    [Header("Failure")]
    [Tooltip("绝对角度达到这个值就失败（只看持久化角度）")]
    [SerializeField] private float _failTiltDeg = 35f;

    [Tooltip("持久化角度最大值（绝对值）")]
    [SerializeField] private float _maxBaseTiltDeg = 50f;

    // [Header("Persistent Tilt (Base)")]
    // [Tooltip("strength=1 的基础累积角度增量")]
    // [SerializeField] private float _baseTiltPerHitDeg = 6f;

    [SerializeField] private float _sideWeight = 1.0f;
    [SerializeField] private float _verticalWeight = 0.35f;

    [Tooltip("每秒自动回正多少度。0=不回正（纯持久化）")]
    [SerializeField] private float _autoRecoverDegPerSec = 0f;

    [Header("Hit Shake (Spring)")]
    [Tooltip("strength=1 时给的晃动冲量大小（度/秒 级别）")]
    [SerializeField] private float _shakeImpulse = 18f;

    [Tooltip("晃动弹簧强度（越大回弹越快）")]
    [SerializeField] private float _shakeSpring = 220f;

    [Tooltip("晃动阻尼（越大越不晃）")]
    [SerializeField] private float _shakeDamping = 34f;

    [Tooltip("晃动最大角度（绝对值）")]
    [SerializeField] private float _maxShakeDeg = 10f;

    [Header("Debug Readonly")]
    [SerializeField] private float _baseTiltDeg; // 持久化角度
    [SerializeField] private float _shakeDeg;    // 临时晃动角度
    [SerializeField] private float _shakeVel;    // 晃动角速度

    [SerializeReference] private Transform playerRoot;
    public float BaseTiltDeg => _baseTiltDeg; // 用这个做失败判定
    public float CurrentVisualTiltDeg => _baseTiltDeg + _shakeDeg;
    public float Fail01 => Mathf.InverseLerp(0f, _failTiltDeg, Mathf.Abs(_baseTiltDeg));
    public bool IsFailed => Mathf.Abs(_baseTiltDeg) >= _failTiltDeg;

    private void Reset()
    {
        _rotRoot = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (_rotRoot == null) _rotRoot = GetComponent<RectTransform>();
        if (_centerRef == null) _centerRef = _rotRoot;
    }

    private void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;

        // 1) 可选：持久化角度慢慢回正（默认 0 不回正）
        if (_autoRecoverDegPerSec > 0f)
        {
            _baseTiltDeg = Mathf.MoveTowards(_baseTiltDeg, 0f, _autoRecoverDegPerSec * dt);
        }

        // 2) 晃动：弹簧阻尼回到 0（回到“当前持久化角度”，不是回到 0 角度）
        float accel = (-_shakeSpring * _shakeDeg) - (_shakeDamping * _shakeVel);
        _shakeVel += accel * dt;
        _shakeDeg += _shakeVel * dt;
        _shakeDeg = Mathf.Clamp(_shakeDeg, -_maxShakeDeg, _maxShakeDeg);

        // 3) 最终显示
        if (Mathf.Abs(_baseTiltDeg) > _maxBaseTiltDeg)
        {
            GlobalShare.EventBus.Publish(new PlayerDeath.PlayerEvtDeath());
        }
        _baseTiltDeg = Mathf.Clamp(_baseTiltDeg, -_maxBaseTiltDeg, _maxBaseTiltDeg);
        float finalTilt = _baseTiltDeg + _shakeDeg;

        _rotRoot.localRotation = Quaternion.Euler(0f, 0f, finalTilt);
    }

    /// <summary>
    /// 命中点：屏幕坐标。strength01：0~1
    /// </summary>
    public void ApplyHitScreenPos(AttackPack pack)
    {
        // var hitScreenPos = pack.HitScreenPos;
        var strength01 = pack.Strength01;
        var _baseTiltPerHitDeg = pack.BaseHit;
            
        // Vector2 center = GetCenterScreenPos();
        // Vector2 dir = hitScreenPos - center;
        // if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        // dir.Normalize();

        // strength01 = Mathf.Clamp01(strength01);

        // float side = Mathf.Clamp(dir.x, -1f, 1f);
        // float vert = Mathf.Clamp(dir.y, -1f, 1f);

        // ✅ 1) 持久化角度：慢慢累积（用于失败）
        // float deltaBase = (-side * _sideWeight + -vert * _verticalWeight) * (_baseTiltPerHitDeg * strength01);
        var pos = pack.AttackerPos;
        var mPos = playerRoot.transform.position;
        var dir = mPos - pos;
        var signedAngle = Vector2.SignedAngle(playerRoot.forward.ConvertXZ(), dir.ConvertXZ());

        var sign = (signedAngle > 0 ? -1 : 1);
        
        var deltaBase = sign * strength01 * _baseTiltPerHitDeg;
        _baseTiltDeg += deltaBase;

        // ✅ 2) 晃动冲量：立即“挨揍那一下”更有劲
        // 用同一个方向决定晃动方向（打右边 -> 往左甩一下）
        // float impulse = (-side * _sideWeight + -vert * _verticalWeight) * (_shakeImpulse * strength01);
        var impulse = sign * _shakeImpulse * strength01;
        _shakeVel += impulse;
        
        //发送特效
        GlobalShare.EventBus.Publish(new HelmetHitEffect3D.HelmetHitEffectData(){AttackerPos = pack.AttackerPos});
    }
    public struct AttackPack
    {
        public Vector2 HitScreenPos;
        public float Strength01;
        public float BaseHit;
        public Vector3 AttackerPos;
    }

    /// <summary>
    /// 命中点：世界坐标（碰撞点/攻击者位置），自动转屏幕坐标
    /// </summary>
    public void ApplyHitWorldPos(Vector3 hitWorldPos, Camera cam, float strength01 = 1f)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector2 sp = cam.WorldToScreenPoint(hitWorldPos);
        ApplyHitScreenPos(new AttackPack()
        {
            HitScreenPos = sp,
            Strength01 = strength01,
            BaseHit = 6
        });
    }

    public void SetBaseTilt(float tiltDeg)
    {
        _baseTiltDeg = tiltDeg;
    }

    public void ClearShake()
    {
        _shakeDeg = 0f;
        _shakeVel = 0f;
    }

    private Vector2 GetCenterScreenPos()
    {
        Vector3 world = _centerRef.TransformPoint(_centerRef.rect.center);
        return RectTransformUtility.WorldToScreenPoint(null, world);
    }
}
