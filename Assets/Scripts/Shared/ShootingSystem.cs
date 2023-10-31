using Shared.Physics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .091ms High: .168ms
 */

namespace Shared
{
    public partial struct ShootingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Shooting>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var shooting in 
                     SystemAPI.Query<ShootingAspect>().WithAll<Shooting>())
            {
                var shootingRO = shooting.Shooting;

                shooting.FireDelayTimer = math.clamp(shootingRO.fireDelayTimer + deltaTime, 0f, shootingRO.fireDelayDuration);
                if (shooting.FireDelayTimer < shootingRO.fireDelayDuration) continue;

                Entity instance = state.EntityManager.Instantiate(shootingRO.projectilePrefab);

                var bulletSpawn = SystemAPI.GetComponent<LocalToWorld>(shootingRO.projectileSpawn);
                
                state.EntityManager.SetComponentData(instance, new LocalTransform
                {
                    Position = bulletSpawn.Position,
                    Rotation = bulletSpawn.Rotation,
                    Scale = SystemAPI.GetComponent<LocalTransform>(shootingRO.projectilePrefab).Scale
                });

                state.EntityManager.SetComponentData(instance, new PhysicsBody
                {
                    moveVector = bulletSpawn.Right.xy,
                });

                if (shooting.HasPhysicsBody())
                {
                    var physics = state.EntityManager.GetComponentData<PhysicsBody>(instance);
                    physics.velocity += shooting.Velocity;
                    state.EntityManager.SetComponentData(instance, physics);
                }
                    
                shooting.FireDelayTimer = 0;
            }
        }
    }

    public struct Shooting : IComponentData, IEnableableComponent
    {
        public Entity projectilePrefab;
        public Entity projectileSpawn;

        public float fireDelayDuration;
        public float fireDelayTimer;
    }
    
    public readonly partial struct ShootingAspect : IAspect
    {
        private readonly RefRW<Shooting> shooting;
        [Optional] private readonly RefRO<PhysicsBody> physicsBody;

        public Shooting Shooting
        {
            get => shooting.ValueRO;
            set => shooting.ValueRW = value;
        }

        public float FireDelayTimer
        {
            get => shooting.ValueRO.fireDelayTimer;
            set => shooting.ValueRW.fireDelayTimer = value;
        }

        public bool HasPhysicsBody() => physicsBody.IsValid;
        public float2 Velocity => physicsBody.ValueRO.velocity;
    }
}