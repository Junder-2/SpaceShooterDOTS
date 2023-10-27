using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Level
{
    public partial struct EnemySpawnerSystem : ISystem
    {
        private EntityQuery spawnerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prefabBuffer = SystemAPI.GetSingletonBuffer<EntityBufferElement>();
            
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (enemySpawner, entity) in SystemAPI.Query<RefRW<EnemySpawner>>().WithEntityAccess())
            {
                Random random = new Random(1);
                
                var enemySpawnerRO = enemySpawner.ValueRO;

                for (int enemyIndex = 0; enemyIndex < enemySpawnerRO.spawnAmount; enemyIndex++)
                {
                    var randomPos = random.NextFloat2Direction() * enemySpawnerRO.spawnRadius + enemySpawnerRO.worldPosition;

                    Entity instance = entityCommandBuffer.Instantiate(prefabBuffer[enemySpawnerRO.prefabIndex]);

                    entityCommandBuffer.SetComponent(instance, new LocalTransform
                    {
                        Position = new float3(randomPos, 0),
                        Rotation = quaternion.identity,
                        Scale = 1f,
                    });
                }

                entityCommandBuffer.SetComponentEnabled<EnemySpawner>(entity, false);
            }
        }
    }
}