using Level;
using Unity.Burst;
using Unity.Entities;

namespace Shared.Physics
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct CollisionDetectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CollisionEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            
            foreach (var (collisionEvent, collider, entity) 
                     in SystemAPI.Query<RefRW<CollisionEvent>, BoxColliderAspect>().WithDisabled<CollisionEvent>().WithEntityAccess())
            {
                if (entityManager.HasComponent<BoxCollider>(collisionEvent.ValueRO.collidedEntity))
                {
                    var otherCollider = SystemAPI.GetAspect<BoxColliderAspect>(collisionEvent.ValueRO.collidedEntity);
                    var otherWorldBounds = otherCollider.GetWorldBounds();

                    if (collider.CheckCollision(otherWorldBounds)) continue;
                }
                
                collisionEvent.ValueRW.isBoundary = false;
                collisionEvent.ValueRW.collidedEntity = Entity.Null;
                entityManager.SetComponentEnabled<CollisionEvent>(entity, true);
            }
            
            foreach (var (collisionEvent, collider, entity)
                     in SystemAPI.Query<RefRW<CollisionEvent>, BoxColliderAspect>().WithEntityAccess())
            {
                var collisionMask = collisionEvent.ValueRO.collisionMask;

                if (SystemAPI.TryGetSingleton(out Boundary boundary))
                {
                    if (collisionMask.CheckLayer(boundary.collisionLayer) &&
                        collider.CheckInverseCollision(boundary.boundingRect))
                    {
                        collisionEvent.ValueRW.isBoundary = true;
                        collisionEvent.ValueRW.collidedEntity = Entity.Null;
                        entityManager.SetComponentEnabled<CollisionEvent>(entity, false);
                        continue;
                    }
                }
                
                foreach (var (otherCollider, otherEntity) in SystemAPI.Query<BoxColliderAspect>().WithEntityAccess())
                {
                    if(entity.Index == otherEntity.Index) continue;

                    var otherBox = otherCollider.BoxCollider;
                    
                    if(!otherBox.raiseCollisionEvents) continue;
                    
                    if(!collisionMask.CheckLayer(otherCollider.BoxCollider.layer)) continue;

                    if (!collider.CheckCollision(otherCollider.GetWorldBounds())) continue;

                    collisionEvent.ValueRW.collidedEntity = otherEntity;
                    collisionEvent.ValueRW.isBoundary = false;
                    entityManager.SetComponentEnabled<CollisionEvent>(entity, false);
                    break;
                }
            }
        }
    }
}