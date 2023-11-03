using ECS.Components.Enemy;
using ECS.Components.Player;
using ECS.Systems.Player;
using ECS.Systems.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using EnemyAspect = ECS.Components.Enemy.EnemyAspect;

/*  Profiler 31.10 Tested 200 enemies
 *  Low: .013ms High: .034ms
 */

namespace ECS.Systems.Enemy
{
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(PlayerProcessSystem))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
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

            foreach (var (enemy, localToWorld, entity) 
                     in SystemAPI.Query<EnemyAspect, LocalToWorld>().
                         WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess().WithAll<EnemyInfo>())
            {
                enemy.TargetPosition = playerPos;
                
                var targetDir = math.normalizesafe(playerPos - localToWorld.Position.xy);

                enemy.physicsBodyAspect.FaceDirection = targetDir;
                enemy.physicsBodyAspect.MoveVector = targetDir;

                if (state.EntityManager.HasComponent<Shooting>(entity))
                {
                    state.EntityManager.SetComponentEnabled<Shooting>(entity, Random.CreateFromIndex((uint)(entity.Index + (time*.5f))).NextBool());
                }
            }
        }
    }
}