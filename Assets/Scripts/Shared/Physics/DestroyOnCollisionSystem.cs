using Unity.Burst;
using Unity.Entities;

namespace Shared.Physics
{
    [UpdateAfter(typeof(CollisionDetectionSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct DestroyOnCollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyOnCollision>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (collisionEvent, entity) 
                     in SystemAPI.Query<CollisionEvent>().WithDisabled<CollisionEvent>().WithAll<DestroyOnCollision>().WithEntityAccess())
            {
                entityCommandBuffer.SetComponentEnabled<Alive>(entity, false);
            }
        }
    }

    public struct DestroyOnCollision : IComponentData
    {
    }
}