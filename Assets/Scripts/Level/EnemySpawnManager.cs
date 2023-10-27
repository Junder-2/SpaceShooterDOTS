using System;
using Enemy;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Level
{
    [Serializable]
    public struct EnemyPrefab
    {
        public GameObject prefab;
        public float requiredDifficulty;
    }
    
    public class EnemySpawnManager : MonoBehaviour
    {
        public static EnemySpawnManager instance;
        
        [SerializeField] private float startingDifficulty = 1f;
        [SerializeField] private float startingEnemies = 8f;
        
        [SerializeField] private float difficultyIncreasePerWave = 1f;
        [SerializeField] private float enemiesPerWave = 5f;
        [SerializeField] private float timeBetweenEnemyWaves = 5f;

        [SerializeField] private float enemySpawnRadius = 5f;

        [SerializeField] private EnemyPrefab[] enemyPrefabs;

        enum EnemySpawnStage
        {
            Spawn, Wait, Delay
        }
        
        private EntityManager entityManager;
        private EntityQuery enemyQuery;
        private EntityQuery levelTagQuery;
        private Entity[] spawnerEntities;

        private EnemySpawnStage enemySpawnStage;
        private float enemySpawnTimer;
        private float currentDifficulty;
        private float currentEnemySpawnCount;
        
        private void Awake()
        {
            instance = this;
        }
        
        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            enemyQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<EnemyInfo>());
            levelTagQuery = entityManager.CreateEntityQuery(ComponentType.ReadWrite<LevelTag>()); 

            enemySpawnStage = EnemySpawnStage.Delay;
            currentEnemySpawnCount = startingEnemies;
            currentDifficulty = startingDifficulty;
            
            spawnerEntities = new Entity[enemyPrefabs.Length];

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                spawnerEntities[i] = entityManager.CreateEntity(ComponentType.ReadWrite<EnemySpawner>());
                entityManager.AddComponentData(spawnerEntities[i], new EnemySpawner
                {
                    prefabIndex = i,
                    spawnRadius = enemySpawnRadius,
                    worldPosition = (Vector2)transform.position
                });
                
                entityManager.SetComponentEnabled<EnemySpawner>(spawnerEntities[i], false);
            }
        }

        private void Update()
        {
            switch (enemySpawnStage)
            {
                case EnemySpawnStage.Spawn:
                    EnemySpawnStageSpawn();
                    break;
                case EnemySpawnStage.Wait:
                    EnemySpawnStageWait();
                    break;
                case EnemySpawnStage.Delay:
                    EnemySpawnStageDelay();
                    break;
            }
        }

        void EnemySpawnStageSpawn()
        {
            if(spawnerEntities == null) return;
            
            for (int i = 0; i < spawnerEntities.Length; i++)
            {
                var spawnComponent = entityManager.GetComponentData<EnemySpawner>(spawnerEntities[i]);
                
                spawnComponent.spawnAmount = (int)currentEnemySpawnCount;
                entityManager.SetComponentData(spawnerEntities[i], spawnComponent);
                entityManager.SetComponentEnabled<EnemySpawner>(spawnerEntities[i], true);
            }

            enemySpawnStage = EnemySpawnStage.Wait;
        }

        void EnemySpawnStageWait()
        {
            if(!enemyQuery.IsEmpty) return;

            enemySpawnTimer = timeBetweenEnemyWaves;
            enemySpawnStage = EnemySpawnStage.Delay;
        }

        void EnemySpawnStageDelay()
        {
            enemySpawnTimer -= Time.deltaTime;
                    
            if(enemySpawnTimer > 0) return;

            currentDifficulty += difficultyIncreasePerWave;
            currentEnemySpawnCount += enemiesPerWave;
            enemySpawnStage = EnemySpawnStage.Spawn;
        }
        
        public EnemyPrefab[] GetEnemyPrefabs() => enemyPrefabs;
    }
}