using System;
using UnityEngine;

public sealed class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Move,
        Charge,
        Attack
    }

    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int maxHp = 3;

    [Header("Behavior")]
    [SerializeField] private Transform target;
    [SerializeField] private float idleDuration = 1f;
    [SerializeField] private float chargeDuration = 0.6f;
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float attackRange = 1.2f;

    public int CurrentHp { get; private set; }
    public EnemyState State => _state;

    public event Action Attacked;

    private EnemyState _state = EnemyState.Idle;
    private float _stateTimer;

    private void Awake()
    {
        CurrentHp = Mathf.Max(1, maxHp);
        EnterState(EnemyState.Idle, idleDuration);
    }

    private void Update()
    {
        switch (_state)
        {
            case EnemyState.Idle:
                TickIdle();
                break;
            case EnemyState.Move:
                TickMove();
                break;
            case EnemyState.Charge:
                TickCharge();
                break;
            case EnemyState.Attack:
                TickAttack();
                break;
        }
    }

    private void TickIdle()
    {
        if (target == null)
        {
            return;
        }

        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
        {
            EnterState(EnemyState.Move);
        }
    }

    private void TickMove()
    {
        if (target == null)
        {
            EnterState(EnemyState.Idle, idleDuration);
            return;
        }

        var pos = transform.position;
        var targetPos = target.position;
        var toTarget = targetPos - pos;
        var sqrDist = toTarget.sqrMagnitude;
        var attackRangeSqr = attackRange * attackRange;

        if (sqrDist <= attackRangeSqr)
        {
            EnterState(EnemyState.Charge, chargeDuration);
            return;
        }

        var step = toTarget.normalized * (moveSpeed * Time.deltaTime);
        transform.position = pos + step;
    }

    private void TickCharge()
    {
        if (target == null)
        {
            EnterState(EnemyState.Idle, idleDuration);
            return;
        }

        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
        {
            EnterState(EnemyState.Attack, attackDuration);
        }
    }

    private void TickAttack()
    {
        if (target == null)
        {
            EnterState(EnemyState.Idle, idleDuration);
            return;
        }

        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
        {
            var sqrDist = (target.position - transform.position).sqrMagnitude;
            var attackRangeSqr = attackRange * attackRange;
            if (sqrDist <= attackRangeSqr)
            {
                EnterState(EnemyState.Charge, chargeDuration);
            }
            else
            {
                EnterState(EnemyState.Move);
            }
        }
    }

    private void EnterState(EnemyState next, float duration = 0f)
    {
        _state = next;
        _stateTimer = duration;

        if (next == EnemyState.Attack)
        {
            Attacked?.Invoke();
        }

        if (next == EnemyState.Idle && _stateTimer <= 0f)
        {
            _stateTimer = idleDuration;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target == null)
        {
            EnterState(EnemyState.Idle, idleDuration);
        }
        else if (_state == EnemyState.Idle)
        {
            EnterState(EnemyState.Move);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHp <= 0)
        {
            return;
        }

        CurrentHp -= amount;
        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHp <= 0)
        {
            return;
        }

        CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHp < 1)
        {
            maxHp = 1;
        }

        if (moveSpeed < 0f)
        {
            moveSpeed = 0f;
        }

        if (attackRange < 0f)
        {
            attackRange = 0f;
        }

        if (idleDuration < 0f)
        {
            idleDuration = 0f;
        }

        if (chargeDuration < 0f)
        {
            chargeDuration = 0f;
        }

        if (attackDuration < 0f)
        {
            attackDuration = 0f;
        }
    }
#endif
}
