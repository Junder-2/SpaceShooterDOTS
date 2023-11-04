using ECS.Components.Player;
using ECS.Components.Shared;
using ECS.Systems.Level;
using ECS.Systems.Player;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    [DefaultExecutionOrder(-1)]
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;

        [SerializeField] private bool restartOnDeath = true;
        
        private EntityQuery playerQuery;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            
            var entityManager = world.EntityManager;
            playerQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerInfo>(), ComponentType.ReadWrite<Health>());
            
            var enemySpawner = world.GetExistingSystemManaged<EnemySpawnerSystem>();
            enemySpawner.OnStartWave += HealPlayer;

            var playerPostProcess = world.GetExistingSystemManaged<PlayerPostProcessSystem>();
            playerPostProcess.OnDeath += PlayerDeath;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                RestartLevel();
            }

            if (Input.GetKeyUp(KeyCode.I))
            {
                restartOnDeath = !restartOnDeath;
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }
        
        private void HealPlayer()
        {
            var healthData = playerQuery.GetSingleton<Health>();
            healthData.currentHealth = healthData.maxHealth;
            
            playerQuery.SetSingleton(healthData);
        }

        private void PlayerDeath()
        {
            if(restartOnDeath) RestartLevel();
        }

        private void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
