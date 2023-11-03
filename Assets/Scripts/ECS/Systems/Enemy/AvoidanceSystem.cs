using ECS.Components.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .075ms High: .379ms few spikes
 */

namespace ECS.Systems.Enemy
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AvoidanceSystem : ISystem
    {
        private EntityQuery avoidanceQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Avoidance>();
            avoidanceQuery = state.GetEntityQuery(ComponentType.ReadOnly<Avoidance>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatialMap = new NativeParallelMultiHashMap<int, EntityPositionData>
                (avoidanceQuery.CalculateEntityCount(), Allocator.TempJob);

            var gatheringJob = new AvoidanceSpatialGathering()
            {
                spatialMapWriter = spatialMap.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency);
            
            gatheringJob.Complete();

            var jobHandle = new AvoidanceJob()
            {
                spatialMap = spatialMap.AsReadOnly(),
                deltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(state.Dependency);
            
            jobHandle.Complete();

            spatialMap.Dispose();
        }
    }

    [BurstCompile]
    public partial struct AvoidanceSpatialGathering : IJobEntity
    {
        public NativeParallelMultiHashMap<int, EntityPositionData>.ParallelWriter spatialMapWriter;

        public void Execute(LocalToWorld localToWorld, Entity entity, Avoidance avoidance)
        {
            int hashKey = EntityPhysics.GetSpatialHashMapKey(localToWorld.Position.xy);
            spatialMapWriter.Add(hashKey, new EntityPositionData
            {
                worldPos = localToWorld.Position.xy,
                entity = entity,
            });
        }
    }

    [BurstCompile]
    public partial struct AvoidanceJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, EntityPositionData>.ReadOnly spatialMap;
        public float deltaTime;

        public void Execute(PhysicsBodyAspect physicsBody, LocalToWorld localToWorld, Avoidance avoidance, Entity entity)
        {
            float2 pushForce = float2.zero;
            
            var cellOffsets = EntityPhysics.CellOffsets;
            var rand = Random.CreateFromIndex((uint)entity.Index);
            
            for (int i = 0; i < cellOffsets.Length; i++)
            {
                int checkKey = EntityPhysics.GetSpatialHashMapKey(localToWorld.Position.xy + cellOffsets[i]);

                if (!spatialMap.TryGetFirstValue(checkKey, out var otherEntityData, out var iterator)) continue;
                do
                {
                    var otherEntity = otherEntityData.entity;
                    var otherPos = otherEntityData.worldPos;
                    
                    if(entity.Index == otherEntity.Index) continue;
                    
                    float2 avoidVector = -(otherPos - localToWorld.Position.xy);

                    float dist = math.length(avoidVector);
                    
                    if(dist > avoidance.pushRadius) continue;

                    float normalizeDist = 1-(dist/avoidance.pushRadius);

                    var randDir = rand.NextFloat2Direction();

                    pushForce += math.normalizesafe(avoidVector, randDir) * (avoidance.pushForce * normalizeDist);
                } while (spatialMap.TryGetNextValue(out otherEntityData, ref iterator));
            }
            
            physicsBody.Velocity += pushForce * deltaTime;
        }
    }

    public struct Avoidance : IComponentData
    {
        public float pushRadius;
        public float pushForce;
    }

    public struct EntityPositionData
    {
        public float2 worldPos;
        public Entity entity;
    }
}