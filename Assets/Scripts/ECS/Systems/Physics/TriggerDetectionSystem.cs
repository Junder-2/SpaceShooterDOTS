using ECS.Components.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .175ms High: .975ms few spikes
 *
 *  Profiler 3.11 Tested 200 enemies
 *  Low: .02ms High: .172ms few spikes
 */

namespace ECS.Systems.Physics
{
    [UpdateAfter(typeof(CollisionDetectionSystem))]
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
                     in SystemAPI.Query<RefRW<TriggerEvent>, Components.Physics.BoxColliderAspect>().WithDisabled<TriggerEvent>().WithEntityAccess())
            {
                if (entityManager.HasComponent<BoxCollider>(triggerEvent.ValueRO.collidedEntity))
                {
                    var otherCollider = SystemAPI.GetAspect<Components.Physics.BoxColliderAspect>(triggerEvent.ValueRO.collidedEntity);
                    var otherWorldBounds = otherCollider.GetWorldBounds();

                    if (collider.CheckCollision(otherWorldBounds)) continue;
                }

                triggerEvent.ValueRW.collidedEntity = Entity.Null;
                entityManager.SetComponentEnabled<TriggerEvent>(entity, true);
            }

            var entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var parallelWriter = entityCommandBuffer.AsParallelWriter();
            var spatialHashMap = SystemAPI.GetSingleton<WorldCollisionInfo>().spatialCollisionMap;

            var jobHandle = new CheckTriggerJob()
            {
                ecb = parallelWriter,
                spatialHashMap = spatialHashMap.AsReadOnly(),
            }.ScheduleParallel(state.Dependency);
            
            jobHandle.Complete();
            
            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }

    [BurstCompile]
    public partial struct CheckTriggerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public NativeParallelMultiHashMap<int, ColliderData>.ReadOnly spatialHashMap;

        public void Execute(ref TriggerEvent triggerEvent, Components.Physics.BoxColliderAspect collider, Entity entity)
        {
            var collisionMask = triggerEvent.collisionMask;
            var colliderBounds = collider.GetWorldBounds();

            var currentCellBounds = EntityPhysics.GetSpatialBounds(colliderBounds.xy);

            for (int i = 0; i < EntityPhysics.CellOffsetLength; i++)
            {
                var otherCellBounds = EntityPhysics.GetSpatialBounds(currentCellBounds.xy + EntityPhysics.GetCellOffset(i));

                if (!EntityPhysics.AABBOverlap(colliderBounds, otherCellBounds)) continue;

                int checkKey = EntityPhysics.GetSpatialHashMapKey(otherCellBounds.xy);

                if (!spatialHashMap.TryGetFirstValue(checkKey, out var otherColliderData, out var iterator)) continue;
                do
                {
                    var otherEntity = otherColliderData.entity;
                    var otherCollider = otherColliderData.boxCollider;
                    
                    if(entity.Index == otherEntity.Index) continue;

                    if(!otherCollider.raiseTriggerEvents) continue;
                    
                    if(!collisionMask.CheckLayer(otherCollider.layer)) continue;

                    if (!EntityPhysics.AABBOverlap(colliderBounds, otherColliderData.worldBounds)) continue;

                    triggerEvent.collidedEntity = otherEntity;
                    ecb.SetComponentEnabled<TriggerEvent>(entity.Index, entity, false);
                    return;
                } while (spatialHashMap.TryGetNextValue(out otherColliderData, ref iterator));
            }
        }
    }
}