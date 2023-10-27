using Unity.Burst;
using Unity.Entities;

namespace Shared
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial struct DestroySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Alive>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (alive, entity) in SystemAPI.Query<Alive>().WithDisabled<Alive>().WithEntityAccess())
            {
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
    
    public struct Alive : IComponentData, IEnableableComponent
    {
    }
}