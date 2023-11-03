using ECS.Components.Level;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Level
{
    public partial class EnemySpawnerSystem : SystemBase
    {
        public int currentDifficulty;
        public int spawnAmount;

        private EntityQuery spawnerQuery;
        
        protected override void OnCreate()
        {
            RequireForUpdate<EnemySpawner>();
            spawnerQuery = GetEntityQuery(ComponentType.ReadOnly<EnemySpawner>());
        }
        
        protected override void OnUpdate()
        {
            if(spawnAmount <= 0) return;
            
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            var random = new Random(1);

            var enemySpawners = spawnerQuery.ToComponentArray<EnemySpawner>();

            for (int i = 0; i < spawnAmount; i++)
            {
                var index = random.NextInt(enemySpawners.Length);

                var enemySpawner = enemySpawners[index];

                var randomPos = random.NextFloat2Direction() * enemySpawner.spawnRadii + enemySpawner.worldPosition;

                Entity instance = entityCommandBuffer.Instantiate(enemySpawner.prefab);

                entityCommandBuffer.SetComponent(instance, new LocalTransform
                {
                    Position = new float3(randomPos, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f,
                });
            }

            spawnAmount = 0;
        }
    }
}