using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemyGroups;
        public int waveQuota;
        public float spawnInterval;
        public float spawnCount;
    }
    
    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public int enemyCount;
        public int spawnCount;
        public GameObject enemyPrefab;
    }
    public List<Wave> waves;
    public int currentWaveCount;

    [Header("Spawner Attributes")]
    float spawnTimer;
    public int enemiesAlive;
    public int maxEnemiesAllowed;
    public bool maxEnemiesreached = false;
    public float waveInterval;
    bool isWaveActive = false;

    [Header("Spawn Postions")]
    public List<Transform> relativeSpawnPoints;

    Transform player;
    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>().transform;
        CalculateWaveQuota();
    }
    void Update()
    {

        if (currentWaveCount < waves.Count && waves[currentWaveCount].spawnCount == 0 && isWaveActive == false)
        {
            StartCoroutine(BeginNextWave());
        }
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= waves[currentWaveCount].spawnInterval)
        {
            spawnTimer = 0;
            SpawnEnemies();
        }
    }
    IEnumerator BeginNextWave()
    {
        isWaveActive = true;
        yield return new WaitForSeconds(waveInterval);
        if(currentWaveCount < waves.Count - 1)
        {
            isWaveActive = false;
            currentWaveCount++;
            CalculateWaveQuota();
        }
    }
    void CalculateWaveQuota()
    {
        int currentWaveQuota = 0;
        foreach (var enemyGroup in waves[currentWaveCount].enemyGroups)
        {
            currentWaveQuota += enemyGroup.enemyCount;
        }
        waves[currentWaveCount].waveQuota = currentWaveQuota;
        Debug.LogWarning(currentWaveQuota);
    }
    void SpawnEnemies()
    {
        if (waves[currentWaveCount].spawnCount < waves[currentWaveCount].waveQuota && !maxEnemiesreached)
        {
            foreach (var enemyGroup in waves[currentWaveCount].enemyGroups)
            {
                if(enemyGroup.spawnCount < enemyGroup.enemyCount)
                {
                    Instantiate(enemyGroup.enemyPrefab, player.position + relativeSpawnPoints[Random.Range(0, relativeSpawnPoints.Count)].position, Quaternion.identity);
                    enemyGroup.spawnCount++;
                    waves[currentWaveCount].spawnCount++;
                    enemiesAlive++;

                    if (enemiesAlive >= maxEnemiesAllowed)
                    {
                        maxEnemiesreached = true;
                        return;
                    }
                }

            }
        }
    }
    public void OnEnemyKill()
    {
        enemiesAlive--;
        if (enemiesAlive < maxEnemiesAllowed)
        {
            maxEnemiesreached = false;
        } 
    }
}