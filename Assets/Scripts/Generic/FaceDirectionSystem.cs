using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace Generic
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct FaceDirectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FaceDirection>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new RotateTowardsJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel();
        }
    }

    public partial struct RotateTowardsJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(FaceDirection faceDirection, ref LocalTransform transform)
        {
            var target = faceDirection.direction;
            var speed = faceDirection.rotateSpeed;
            
            if(target.Equals(float2.zero)) return;

            var forward = transform.Right();

            float currentAngle = math.atan2(forward.y, forward.x);
            float targetAngle = math.atan2(target.y, target.x);

            var tau = math.PI * 2f;

            transform.Rotation = quaternion.RotateZ(DOTSMath.MoveTowardsRadians(currentAngle, targetAngle, speed * deltaTime * tau));
        }
    }

    public struct FaceDirection : IComponentData
    {
        public float2 direction;
        public float rotateSpeed;
    }
}