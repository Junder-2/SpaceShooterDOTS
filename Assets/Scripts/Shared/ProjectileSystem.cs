using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Shared
{
    public partial struct ProjectileSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Projectile>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            new ProjectileJob
            {
                entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
                deltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule();
        }
    }

    [BurstCompile]
    public partial struct ProjectileJob : IJobEntity
    {
        public EntityCommandBuffer entityCommandBuffer;
        public float deltaTime;

        void Execute(Entity entity, ProjectileAspect projectileAspect)
        {
            projectileAspect.LifeTimer += deltaTime;

            if (projectileAspect.LifeTimer > projectileAspect.Projectile.lifeTimeDuration)
            {
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }

    public struct Projectile : IComponentData
    {
        public float damage;
        public float lifeTimeDuration;
        public float lifeTimer;
    }
    
    public readonly partial struct ProjectileAspect : IAspect
    {
        private readonly RefRW<Projectile> projectile;
        private readonly RefRW<PhysicsBody> physicsBody;
        
        public Projectile Projectile
        {
            get => projectile.ValueRO;
            set => projectile.ValueRW = value;
        }

        public PhysicsBody PhysicsBody
        {
            get => physicsBody.ValueRO; 
            set => physicsBody.ValueRW = value;
        }

        public float LifeTimer
        {
            get => projectile.ValueRO.lifeTimer;
            set => projectile.ValueRW.lifeTimer = value;
        }
    }
}