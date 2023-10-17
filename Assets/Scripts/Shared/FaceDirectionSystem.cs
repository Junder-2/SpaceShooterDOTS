using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace Shared
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
    
    [BurstCompile]
    public partial struct RotateTowardsJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(RotateFacingAspect rotateFacingAspect, ref LocalTransform transform)
        {
            var target = rotateFacingAspect.FaceDirection;
            var speed = rotateFacingAspect.RotateSpeed;
            
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
        public float rotateSpeed;
    }

    public readonly partial struct RotateFacingAspect : IAspect
    {
        private readonly RefRO<FaceDirection> faceDirection;
        private readonly RefRO<PhysicsBody> physicsBody;

        public float2 FaceDirection => physicsBody.ValueRO.faceDirection;
        public float RotateSpeed => faceDirection.ValueRO.rotateSpeed;
    }
}