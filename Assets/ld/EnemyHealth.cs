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
    private Tween _colorTween;
    private Tween _motionTween;
    
    public void TakeDamage(float dmg)
    {
        _hp -= dmg;
        
        //受击效果
        _colorTween.Complete();
        _colorTween = Tween.Color(spriteRenderer, Color.red, spriteRenderer.color, .3f, Ease.InBounce);
        
        _motionTween.Stop();
        _motionTween = Tween.PositionZ(transform, transform.position.z + 1, 1, Ease.OutCubic);
        
        if (_hp <= 0f)
        {
            Destroy(gameObject);
        }
    }
}