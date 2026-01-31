using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float dmg);
}

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float _hp = 3f;

    public void TakeDamage(float dmg)
    {
        _hp -= dmg;
        if (_hp <= 0f)
        {
            Destroy(gameObject);
        }
    }
}