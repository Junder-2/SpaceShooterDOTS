using Player;
using Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemy
{
    public partial struct EnemyProcessSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyInfo>();
            state.RequireForUpdate<PlayerInfo>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var time = SystemAPI.Time.ElapsedTime;
            var playerPos = float2.zero;

            foreach (var playerTransform in SystemAPI.Query<LocalToWorld>().WithAll<PlayerInfo>())
            {
                playerPos = playerTransform.Position.xy;
            }

            foreach (var (enemy, shooting, localToWorld, entity) 
                     in SystemAPI.Query<EnemyAspect, EnabledRefRW<Shooting>, LocalToWorld>().
                         WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                enemy.TargetPosition = playerPos;
                
                var targetDir = math.normalizesafe(playerPos - localToWorld.Position.xy);

                enemy.physicsBodyAspect.FaceDirection = targetDir;
                enemy.physicsBodyAspect.MoveVector = targetDir;
                
                shooting.ValueRW = Random.CreateFromIndex((uint)(entity.Index + (time*.5f))).NextBool();
            }
        }
    }
}