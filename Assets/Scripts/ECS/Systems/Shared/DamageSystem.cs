using ECS.Components.Physics;
using ECS.Components.Shared;
using Unity.Burst;
using Unity.Entities;

namespace ECS.Systems.Shared
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Health>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (health, triggerEvent, entity) 
                     in SystemAPI.Query<RefRW<Health>, TriggerEvent>().WithDisabled<TriggerEvent>().WithEntityAccess())
            {
                var otherEntity = triggerEvent.collidedEntity;
                if (!SystemAPI.HasComponent<Damage>(otherEntity)) continue;
                
                var damage = SystemAPI.GetComponent<Damage>(otherEntity);

                var currHealth = health.ValueRO.currentHealth;

                currHealth -= damage.damageAmount;

                health.ValueRW.currentHealth = currHealth;
                
                if(currHealth <= 0 && health.ValueRO.autoDestroy) entityCommandBuffer.SetComponentEnabled<Alive>(entity, false);
                entityCommandBuffer.SetComponentEnabled<TriggerEvent>(entity, true);
                
                if(damage.autoDestroy) entityCommandBuffer.SetComponentEnabled<Alive>(otherEntity, false);
                entityCommandBuffer.SetComponentEnabled<Damage>(otherEntity, false);
            }
        }
    }
}