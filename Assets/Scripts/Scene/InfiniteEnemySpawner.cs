using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scene
{
    public sealed class InfiniteEnemySpawner : MonoBehaviour
    {
        [Header("Spawn Points")]
        [Tooltip("场景中可用的生成点")]
        [SerializeField] private List<Transform> _spawnPoints = new();

        [Header("Enemy Pool")]
        [SerializeField] private List<GameObject> _enemyPrefabs = new();

        [Header("Spawn Control")]
        [SerializeField] private int _maxAliveEnemies = 5;
        [SerializeField] private float _spawnInterval = 3.0f;
        [SerializeField] private float _minSpawnInterval = 0.8f;

        [Header("Difficulty Scaling")]
        [SerializeField] private float _intervalDecreasePerMinute = 0.4f;
        [SerializeField] private int _maxAliveIncreasePerMinute = 1;

        [Header("Runtime")]
        [SerializeField] private Transform _enemyRoot;

        private readonly List<GameObject> _aliveEnemies = new();
        private float _elapsedTime;
        private float _nextSpawnTime;

        private void Awake()
        {
            if (_enemyRoot == null)
            {
                _enemyRoot = new GameObject("Enemies").transform;
            }
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            CleanupDeadEnemies();
            UpdateDifficulty();

            if (Time.time >= _nextSpawnTime)
            {
                TrySpawnEnemy();
            }
        }

        private void TrySpawnEnemy()
        {
            if (_aliveEnemies.Count >= _maxAliveEnemies)
                return;

            if (_spawnPoints.Count == 0 || _enemyPrefabs.Count == 0)
                return;

            var point = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];

            var enemy = Instantiate(prefab, point.position, Quaternion.identity, _enemyRoot);
            _aliveEnemies.Add(enemy);

            _nextSpawnTime = Time.time + _spawnInterval;
        }

        private void CleanupDeadEnemies()
        {
            // 清理被销毁的敌人
            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (_aliveEnemies[i] == null)
                {
                    _aliveEnemies.RemoveAt(i);
                }
            }
        }

        private void UpdateDifficulty()
        {
            float minutes = _elapsedTime / 60f;

            _spawnInterval = Mathf.Max(
                _minSpawnInterval,
                _spawnInterval - _intervalDecreasePerMinute * minutes * Time.deltaTime
            );

            _maxAliveEnemies = Mathf.Max(
                1,
                _maxAliveEnemies + Mathf.FloorToInt(_maxAliveIncreasePerMinute * Time.deltaTime / 60f)
            );
        }
    }
}
