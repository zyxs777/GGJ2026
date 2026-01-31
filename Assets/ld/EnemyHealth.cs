using PrimeTween;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float dmg);
}

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeReference] private SpriteRenderer spriteRenderer;
    [SerializeField] private float _hp = 3f;
    [SerializeField] private float hitDistance = 1f;
    private Tween _colorTween;
    private Tween _motionTween;
    private Tween _deadTween;
    public void TakeDamage(float dmg)
    {
        if (_deadTween.isAlive) return;
        _hp -= dmg;
        
        //受击效果
        _colorTween.Complete();
        _colorTween = Tween.Color(spriteRenderer, Color.red, spriteRenderer.color, .3f, Ease.InBounce);
        
        _motionTween.Stop();
        _motionTween = Tween.PositionZ(transform, transform.position.z + hitDistance, 1, Ease.OutCubic);
        
        if (_hp <= 0f)
        {
            _deadTween = Tween.PositionZ(transform, transform.position.z + 2 * hitDistance, 1, Ease.OutCubic)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}