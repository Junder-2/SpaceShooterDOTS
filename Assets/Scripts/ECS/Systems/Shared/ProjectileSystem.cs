using ECS.Components.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.Shared
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
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
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var job = new ProjectileJob
            {
                entityCommandBuffer = ecb,
                deltaTime = SystemAPI.Time.DeltaTime,
            }.Schedule(state.Dependency);
            
            job.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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
                entityCommandBuffer.SetComponentEnabled<Alive>(entity, false);
            }
        }
    }

    public struct Projectile : IComponentData
    {
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