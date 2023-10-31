using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PhysicsBodyAspect = Shared.Physics.PhysicsBodyAspect;
using Random = Unity.Mathematics.Random;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .075ms High: .379ms few spikes
 */

namespace Enemy
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AvoidanceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Avoidance>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (physicsBody, localToWorld, avoidance, entity) in SystemAPI.Query<PhysicsBodyAspect, LocalToWorld, Avoidance>().WithAll<Avoidance>().WithEntityAccess())
            {
                float2 pushForce = float2.zero;
                foreach (var (otherLocalToWorld, otherEntity) in SystemAPI.Query<LocalToWorld>().WithAll<Avoidance>().WithEntityAccess())
                {
                    if(entity.Index == otherEntity.Index) continue;
                    
                    float2 avoidVector = -(otherLocalToWorld.Position - localToWorld.Position).xy;

                    float dist = math.length(avoidVector);
                    
                    if(dist > avoidance.pushRadius) continue;

                    float normalizeDist = 1-(dist/avoidance.pushRadius);

                    var rand = Random.CreateFromIndex((uint)otherEntity.Index);

                    var randDir = rand.NextFloat2Direction();

                    pushForce += math.normalizesafe(avoidVector, randDir) * (avoidance.pushForce * normalizeDist);
                }
                
                
                physicsBody.Velocity += pushForce * deltaTime;
            }
        }
    }

    /*[BurstCompile]
    public partial struct RotateTowardsJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(Avoidance rotateFacingAspect, LocalTransform transform, PhysicsBodyAspect physicsBodyAspect)
        {
            
        }

    }*/

    public struct Avoidance : IComponentData
    {
        public float pushRadius;
        public float pushForce;
    }
}