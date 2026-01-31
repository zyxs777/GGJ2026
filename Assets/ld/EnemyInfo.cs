using UnityEngine;

public class EnemyInfo : MonoBehaviour
{
    [SerializeField] private string _enemyName = "Enemy";
    public string EnemyName => _enemyName;
}