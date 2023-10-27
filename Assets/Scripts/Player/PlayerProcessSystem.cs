using System;
using Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Player
{
    [UpdateAfter(typeof(InputGatheringSystem))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlayerProcessSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerInfo>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (player, shooting, localToWorld) 
                     in SystemAPI.Query<PlayerAspect, EnabledRefRW<Shooting>, LocalToWorld>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                var inputData = player.GetInputData();
                
                player.physicsBodyAspect.FaceDirection = math.normalizesafe(inputData.mouseWorldPos - localToWorld.Position).xy;
                player.physicsBodyAspect.MoveVector = inputData.movement;
                
                shooting.ValueRW = inputData.fire;
            }
        }
    }
}