using ECS.Components.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/*  Profiler 3.11 Tested 200 enemies
 *  Low: .066ms High: .2ms few spikes
 */

namespace ECS.Systems.Physics
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class CollisionGatheringSystem : SystemBase
    {
        private EntityQuery collidersQuery;
        private EntityQuery collisionDataQuery;

        protected override void OnCreate()
        {
            collidersQuery = GetEntityQuery(ComponentType.ReadOnly<BoxCollider>());
            collisionDataQuery = GetEntityQuery(typeof(WorldCollisionInfo));
        }
        
        protected override void OnUpdate()
        {
            if (collisionDataQuery.CalculateEntityCount() == 0)
                EntityManager.CreateSingleton<WorldCollisionInfo>();

            var spatialCollisionMap = new NativeParallelMultiHashMap<int, ColliderData>
                (collidersQuery.CalculateEntityCount(), Allocator.TempJob);

            var jobHandle = new CollectCollisionJob()
            {
                spatialCollisionMapWriter = spatialCollisionMap.AsParallelWriter(),
            }.ScheduleParallel(Dependency);
            
            
            jobHandle.Complete();

            collisionDataQuery.SetSingleton(new WorldCollisionInfo
            {
                spatialCollisionMap = spatialCollisionMap,
            });
        }
    }

    [BurstCompile]
    public partial struct CollectCollisionJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, ColliderData>.ParallelWriter spatialCollisionMapWriter;

        public void Execute(Components.Physics.BoxColliderAspect colliderAspect, Entity entity)
        {
            int hashKey = EntityPhysics.GetSpatialHashMapKey(colliderAspect.LocalToWorld.Position.xy);
            spatialCollisionMapWriter.Add(hashKey, new ColliderData
            {
                boxCollider = colliderAspect.BoxCollider,
                worldBounds = colliderAspect.GetWorldBounds(),
                entity = entity,
            });
        }
    }

    public struct WorldCollisionInfo : IComponentData
    {
        public NativeParallelMultiHashMap<int, ColliderData> spatialCollisionMap;
    }
}