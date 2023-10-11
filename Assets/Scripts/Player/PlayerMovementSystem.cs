using Generic;
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
            state.RequireForUpdate<Player>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (player, transform) in SystemAPI.Query<RefRW<Player>, RefRW<LocalTransform>>().WithAll<Player>())
            {
                var playerRO = player.ValueRO;
                var moveInput = playerRO.input.movement;

                var velocity = playerRO.velocity;

                if (math.length(velocity) < playerRO.maxMoveSpeed)
                {
                    velocity += moveInput * (deltaTime * playerRO.accelerationSpeed);
                }
                
                float moveSignX = math.sign(velocity.x);
                float moveSignY = math.sign(velocity.y);

                velocity -= math.normalizesafe(velocity, float2.zero) * (deltaTime * playerRO.decelerationSpeed);

                player.ValueRW.velocity = velocity;

                transform.ValueRW.Position += new float3(velocity.x, velocity.y, 0) * (deltaTime);
            }

            foreach (var (player, localToWorld, faceDirection) in SystemAPI.Query<Player, LocalToWorld, RefRW<FaceDirection>>().WithAll<Player>())
            {
                //faceDirection.ValueRW.direction = player.input.lookDir;
                faceDirection.ValueRW.direction = math.normalize(player.input.mouseWorldPos - localToWorld.Position).xy;
            }
        }
    }
}