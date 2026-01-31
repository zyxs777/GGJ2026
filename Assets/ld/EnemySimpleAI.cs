using UnityEngine;

public class EnemySimpleAI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _player;                       // 玩家Transform
    [SerializeField] private Camera _cam;                             // 主相机
    [SerializeField] private HelmetHitTiltPersistentShake _helmet;    // 头盔受击脚本（你现有的）

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 2.8f;     // 靠近速度
    [SerializeField] private float _stopRange = 1.8f;     // 到这个距离停下
    [SerializeField] private bool _facePlayer = true;     // 是否面向玩家（XZ平面）

    [Header("Attack")]
    [SerializeField] private float _attackRange = 2.2f;   // 能攻击的距离
    [SerializeField] private Vector2 _attackIntervalRange = new Vector2(0.6f, 1.4f); // 随机攻击间隔
    [SerializeField] private Vector2 _strengthRange = new Vector2(0.6f, 1.0f);       // 随机强度

    [Header("Line of Sight (optional)")]
    [Tooltip("敌人不在屏幕里就不攻击（避免背后打但玩家看不到）")]
    [SerializeField] private bool _onlyAttackWhenVisible = true;

    private float _nextAttackTime;

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;

        if (_player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        if (_helmet == null)
        {
            _helmet = FindObjectOfType<HelmetHitTiltPersistentShake>();
        }

        ScheduleNextAttack();
    }

    private void Update()
    {
        if (_player == null) return;

        // 1) 简单靠近（XZ平面）
        Vector3 to = _player.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;

        if (dist > _stopRange)
        {
            Vector3 dir = to / Mathf.Max(0.0001f, dist);
            transform.position += dir * (_moveSpeed * Time.deltaTime);
        }

        if (_facePlayer && dist > 0.01f)
        {
            Vector3 dir = to.normalized;
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // 2) 攻击
        if (dist <= _attackRange && Time.time >= _nextAttackTime)
        {
            if (!_onlyAttackWhenVisible || IsOnScreen())
            {
                DoAttack();
            }
            ScheduleNextAttack();
        }
    }

    private void DoAttack()
    {
        if (_helmet == null || _cam == null) return;

        // ✅ 攻击位置 = 敌人相对头盔中心的位置
        // 本质：把敌人世界坐标投影到屏幕坐标，交给头盔受击逻辑算方向
        Vector2 screenPos = _cam.WorldToScreenPoint(transform.position);

        float strength = Random.Range(_strengthRange.x, _strengthRange.y);

        // 你头盔脚本里如果是 ApplyHitScreenPos 就用这个：
        _helmet.ApplyHitScreenPos(screenPos, strength);

        // （如果你只保留了 ApplyHitWorldPos，也可以改成：）
        // _helmet.ApplyHitWorldPos(transform.position, _cam, strength);
    }

    private void ScheduleNextAttack()
    {
        float dt = Random.Range(_attackIntervalRange.x, _attackIntervalRange.y);
        _nextAttackTime = Time.time + dt;
    }

    private bool IsOnScreen()
    {
        if (_cam == null) return true;

        Vector3 sp = _cam.WorldToViewportPoint(transform.position);
        // z < 0 表示在相机背后
        if (sp.z <= 0f) return false;
        return sp.x >= 0f && sp.x <= 1f && sp.y >= 0f && sp.y <= 1f;
    }
}
