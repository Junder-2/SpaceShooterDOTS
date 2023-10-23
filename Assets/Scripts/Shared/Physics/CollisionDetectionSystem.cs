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
            foreach (var (collisionEvent, collider, entity) 
                     in SystemAPI.Query<RefRW<CollisionEvent>, BoxColliderAspect>().WithDisabled<CollisionEvent>().WithEntityAccess())
            {
                if (!collisionEvent.ValueRO.isBoundary)
                {
                    var otherCollider = SystemAPI.GetAspect<BoxColliderAspect>(collisionEvent.ValueRO.collidedEntity);
                    var otherWorldBounds = otherCollider.GetWorldBounds();

                    if (collider.CheckCollision(otherWorldBounds)) continue;
                
                    collisionEvent.ValueRW.isBoundary = false;
                    state.EntityManager.SetComponentEnabled<CollisionEvent>(entity, true);
                }
                else
                {
                    collisionEvent.ValueRW.isBoundary = false;
                    state.EntityManager.SetComponentEnabled<CollisionEvent>(entity, true);
                }
            }
            
            foreach (var (collisionEvent, collider, entity)
                     in SystemAPI.Query<RefRW<CollisionEvent>, BoxColliderAspect>().WithEntityAccess())
            {
                var collisionMask = collisionEvent.ValueRO.collisionMask;

                if (SystemAPI.TryGetSingleton(out Boundary boundary))
                {
                    if (collisionMask.CheckLayer(boundary.collisionLayer) &&
                        !collider.CheckCollision(boundary.boundingRect))
                    {
                        collisionEvent.ValueRW.isBoundary = true;
                        state.EntityManager.SetComponentEnabled<CollisionEvent>(entity, false);
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
                    state.EntityManager.SetComponentEnabled<CollisionEvent>(entity, false);
                    break;
                }
            }
        }
    }
}