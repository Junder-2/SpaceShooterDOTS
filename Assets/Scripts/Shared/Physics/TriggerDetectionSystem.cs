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
            var entityManager = state.EntityManager;
            
            foreach (var (triggerEvent, collider, entity)
                     in SystemAPI.Query<RefRW<TriggerEvent>, BoxColliderAspect>().WithDisabled<TriggerEvent>().WithEntityAccess())
            {
                if (entityManager.HasComponent<BoxCollider>(triggerEvent.ValueRO.collidedEntity))
                {
                    var otherCollider = SystemAPI.GetAspect<BoxColliderAspect>(triggerEvent.ValueRO.collidedEntity);
                    var otherWorldBounds = otherCollider.GetWorldBounds();

                    if (collider.CheckCollision(otherWorldBounds)) continue;
                }

                triggerEvent.ValueRW.collidedEntity = Entity.Null;
                entityManager.SetComponentEnabled<TriggerEvent>(entity, true);
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
                    entityManager.SetComponentEnabled<TriggerEvent>(entity, false);
                }
            }
        }
    }
}