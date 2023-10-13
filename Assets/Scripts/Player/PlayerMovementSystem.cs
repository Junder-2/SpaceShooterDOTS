using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Player
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerInfo>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (player, transform) in SystemAPI.Query<PlayerAspect, RefRW<LocalTransform>>())
            {
                var playerInfo = player.PlayerInfo;
                var inputData = player.InputData;
                var entityData = player.EntityData;
                var physicsData = player.PhysicsData;

                var velocity = physicsData.velocity;

                if (math.length(velocity) < playerInfo.maxMoveSpeed)
                {
                    velocity += inputData.movement * (deltaTime * playerInfo.accelerationSpeed);
                }

                velocity -= math.normalizesafe(velocity, float2.zero) * (deltaTime * playerInfo.decelerationSpeed);

                physicsData.velocity = velocity;

                transform.ValueRW.Position += new float3(velocity.x, velocity.y, 0) * (deltaTime);
                
                entityData.faceDirection = math.normalizesafe(inputData.mouseWorldPos - transform.ValueRO.Position).xy;
                
                player.PhysicsData = physicsData;
                player.EntityData = entityData;
            }
        }
    }
}