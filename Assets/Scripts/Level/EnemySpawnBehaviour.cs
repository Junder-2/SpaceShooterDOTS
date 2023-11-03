using Enemy;
using Unity.Entities;
using UnityEngine;

namespace Level
{
    [DefaultExecutionOrder(-1)]
    public class EnemySpawnBehaviour : MonoBehaviour
    {
        private static EnemySpawnBehaviour instance;
        public static EnemySpawnBehaviour Instance {
            get {
                return instance ??= FindObjectOfType<EnemySpawnBehaviour>();
            }
        }

        [SerializeField] private bool debugSpawn = false;
        [SerializeField] private int debugSpawnAmount = 100;
        
        [SerializeField] private float startingDifficulty = 1f;
        [SerializeField] private float startingEnemies = 8f;
        
        [SerializeField] private float difficultyIncreasePerWave = 1f;
        [SerializeField] private float enemiesPerWave = 5f;
        [SerializeField] private float timeBetweenEnemyWaves = 5f;
        
        enum EnemySpawnStage
        {
            Spawn, Wait, Delay
        }
        
        private EntityManager entityManager;
        private EntityQuery enemyQuery;
        private EnemySpawnerSystem spawnerSystem;
            
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
            spawnerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EnemySpawnerSystem>(); 

            enemySpawnStage = EnemySpawnStage.Delay;
            currentEnemySpawnCount = startingEnemies;
            currentDifficulty = startingDifficulty;
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
            if(spawnerSystem == null) return;

            spawnerSystem.currentDifficulty = Mathf.FloorToInt(currentDifficulty);
            spawnerSystem.spawnAmount = debugSpawn ? debugSpawnAmount : Mathf.FloorToInt(currentEnemySpawnCount);

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
    }
}