using Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Player
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(InputGatheringSystem))]
    public partial struct PlayerInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerInput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new InputJob
            {
                // Read user input
                input = SystemAPI.GetSingleton<PlayerInput>()
            }.ScheduleParallel(state.Dependency);
        }
    }
    
    public partial struct InputJob : IJobEntity
    {
        public PlayerInput input;

        public void Execute(LocalToWorld localToWorld, ref Player player)
        {
            player.input.movement = input.movement;
            player.input.lookDir = input.lookDir;
            player.input.mousePos = input.mousePos;
            player.input.mouseWorldPos = input.mouseWorldPos;
        }
    }
}