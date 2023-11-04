using ECS.Components.Level;
using ECS.Components.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .7ms High: 4.5ms few spikes
 *
 *  Profiler 3.11 Tested 200 enemies
 *  Low: .04ms High: .25ms few spikes
 */

namespace ECS.Systems.Physics
{
    [UpdateAfter(typeof(CollisionGatheringSystem))]
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

            var entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var parallelWriter = entityCommandBuffer.AsParallelWriter();
            var spatialHashMap = SystemAPI.GetSingleton<WorldCollisionInfo>().spatialCollisionMap;

            var jobHandle = new CheckCollisionJob()
            {
                ecb = parallelWriter,
                spatialHashMap = spatialHashMap.AsReadOnly(),
                boundary = SystemAPI.GetSingleton<Boundary>()
            }.ScheduleParallel(state.Dependency);
            
            jobHandle.Complete();
            
            entityCommandBuffer.Playback(entityManager);
            entityCommandBuffer.Dispose();
        }
    }

    [BurstCompile]
    public partial struct CheckCollisionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public NativeParallelMultiHashMap<int, ColliderData>.ReadOnly spatialHashMap;
        public Boundary boundary;

        public void Execute(ref CollisionEvent collisionEvent, BoxColliderAspect collider, Entity entity)
        {
            var collisionMask = collisionEvent.collisionMask;
            var colliderBounds = collider.GetWorldBounds();
            
            if (collisionMask.CheckLayer(boundary.collisionLayer) && collider.CheckInverseCollision(boundary.boundingRect))
            {
                collisionEvent.isBoundary = true;
                collisionEvent.collidedEntity = Entity.Null;
                ecb.SetComponentEnabled<CollisionEvent>(entity.Index, entity, false);
                return;
            }
            
            var currentCellBounds = EntityPhysics.GetSpatialBounds(colliderBounds.xy);

            for (int i = 0; i < EntityPhysics.CellOffsetLength; i++)
            {
                var otherCellBounds = EntityPhysics.GetSpatialBounds(currentCellBounds.xy + EntityPhysics.GetCellOffset(i));
                
                if(!EntityPhysics.AABBOverlap(colliderBounds, otherCellBounds)) continue;
                
                int checkKey = EntityPhysics.GetSpatialHashMapKey(otherCellBounds.xy);

                if (!spatialHashMap.TryGetFirstValue(checkKey, out var otherColliderData, out var iterator)) continue;
                do
                {
                    var otherEntity = otherColliderData.entity;
                    var otherCollider = otherColliderData.boxCollider;
                    
                    if(entity.Index == otherEntity.Index) continue;

                    if(!otherCollider.raiseCollisionEvents) continue;
                    
                    if(!collisionMask.CheckLayer(otherCollider.layer)) continue;

                    if (!EntityPhysics.AABBOverlap(colliderBounds, otherColliderData.worldBounds)) continue;

                    collisionEvent.collidedEntity = otherEntity;
                    collisionEvent.isBoundary = false;
                    ecb.SetComponentEnabled<CollisionEvent>(entity.Index, entity, false);
                    return;
                } while (spatialHashMap.TryGetNextValue(out otherColliderData, ref iterator));
            }
        }
    }
}