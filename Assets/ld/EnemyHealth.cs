using System;
using PrimeTween;
using Scene;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using STool;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamagePack dmg);
    
    public struct DamagePack
    {
        public Vector3 AttackerPos;
        public float Damage;
    }
}

public class EnemyHealth : SerializedMonoBehaviour, IDamageable
{
    [SerializeReference] private SpriteRenderer spriteRenderer;
    [SerializeField] private float _hp = 3f;
    [SerializeField] private float hitDistance = 1f;
    [NonSerialized] private Tween _colorTween;
    [NonSerialized] private Tween _motionTween;
    [NonSerialized] private Tween _deadTween;
    [OdinSerialize] private IPhyMotion _phyMotion;
    public void TakeDamage(IDamageable.DamagePack dmg)
    {
        if (_deadTween.isAlive) return;
        var hitImpact = (transform.position - dmg.AttackerPos).SetY(0);
        _hp -= dmg.Damage;
        
        //受击效果
        _colorTween.Complete();
        _colorTween = Tween.Color(spriteRenderer, Color.red, spriteRenderer.color, .3f, Ease.InBounce);
        
        _phyMotion.AddImpulse(hitImpact.normalized * hitDistance);
        
        if (_hp <= 0f)
        {
            _deadTween = Tween.PositionZ(transform, transform.position.z + 2 * hitDistance, 1, Ease.OutCubic)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}