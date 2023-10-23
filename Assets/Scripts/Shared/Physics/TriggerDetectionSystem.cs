using Unity.Burst;
using Unity.Entities;

namespace Shared.Physics
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct TriggerDetectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TriggerEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (triggerEvent, collider, entity) 
                     in SystemAPI.Query<RefRW<TriggerEvent>, BoxColliderAspect>().WithDisabled<TriggerEvent>().WithEntityAccess())
            {
                var otherCollider = SystemAPI.GetAspect<BoxColliderAspect>(triggerEvent.ValueRO.collidedEntity);
                var otherWorldBounds = otherCollider.GetWorldBounds();

                if (collider.CheckCollision(otherWorldBounds)) continue;
                
                triggerEvent.ValueRW.collidedEntity = Entity.Null;
                entityCommandBuffer.SetComponentEnabled<TriggerEvent>(entity, true);
            }
            
            foreach (var (triggerEvent, collider, entity) 
                     in SystemAPI.Query<RefRW<TriggerEvent>, BoxColliderAspect>().WithEntityAccess())
            {
                foreach (var (otherCollider, otherEntity) in SystemAPI.Query<BoxColliderAspect>().WithEntityAccess())
                {
                    if(entity.Index == otherEntity.Index) continue;

                    var otherBox = otherCollider.BoxCollider;
                    
                    if(!otherBox.raiseTriggerEvents) continue;
                    
                    if(!triggerEvent.ValueRO.collisionMask.CheckLayer(otherCollider.BoxCollider.layer)) continue;

                    if (!collider.CheckCollision(otherCollider.GetWorldBounds())) continue;

                    triggerEvent.ValueRW.collidedEntity = otherEntity;
                    entityCommandBuffer.SetComponentEnabled<TriggerEvent>(entity, false);
                }
            }
        }
    }
}