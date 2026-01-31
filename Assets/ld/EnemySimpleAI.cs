using UnityEngine;

public class EnemySimpleAI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HelmetHitTiltPersistentShake _helmet;
    [SerializeField] private Camera _cam;

    [Header("Z Move")]
    [SerializeField] private float _moveSpeed = 3.0f;
    [SerializeField] private float _startZ = 12f;     // 出生时的远处 Z
    [SerializeField] private float _attackZ = 2.0f;   // 到这个 Z 就开始攻击
    [SerializeField] private float _stopZ = 1.6f;     // 停下来的最近 Z

    [Header("X Offset")]
    [SerializeField] private float _xOffsetRange = 3.0f; // 左右随机范围

    [Header("Attack")]
    [SerializeField] private Vector2 _attackInterval = new Vector2(0.6f, 1.4f);
    [SerializeField] private Vector2 _attackStrength = new Vector2(0.6f, 1.0f);

    
    [SerializeField] private HelmetAttackIndicator _indicator;
    [SerializeField] private Camera _worldCam;
    private float _nextAttackTime;

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
        if (_helmet == null) _helmet = FindObjectOfType<HelmetHitTiltPersistentShake>();
        if (_worldCam == null) _worldCam = Camera.main;
        if (_indicator == null) _indicator = FindObjectOfType<HelmetAttackIndicator>();

        // ✅ 出生位置：只随机 X，Z 在远处
        Vector3 p = transform.position;
        p.x = Random.Range(-_xOffsetRange, _xOffsetRange);
        p.z = _startZ;
        transform.position = p;

        ScheduleNextAttack();
    }

    private void Update()
    {
        Vector3 p = transform.position;

        // 1️⃣ 沿 Z 轴向玩家靠近
        if (p.z > _stopZ)
        {
            p.z -= _moveSpeed * Time.deltaTime;
            transform.position = p;
        }

        // 2️⃣ 到攻击区间后随机攻击
        if (p.z <= _attackZ && Time.time >= _nextAttackTime)
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

        _helmet.ApplyHitScreenPos(screenPos, strength);
        _indicator?.ShowAtWorldPos(transform.position, _worldCam);
    }

    private void ScheduleNextAttack()
    {
        _nextAttackTime = Time.time + Random.Range(_attackInterval.x, _attackInterval.y);
    }
}
