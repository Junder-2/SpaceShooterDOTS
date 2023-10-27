using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Shared.Physics
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct PhysicsMoveBodySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovePhysicsBody>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new MovePhysicsBodyJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
            
            state.CompleteDependency();
        }
    }
    
    [BurstCompile]
    public partial struct MovePhysicsBodyJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(PhysicsMoveBodyAspect moveBody, ref LocalTransform transform)
        {
            var moveInfo = moveBody.MovePhysicsBody;
            var moveInput = moveBody.physicBodyAspect.MoveVector;
            var velocity = moveBody.physicBodyAspect.Velocity;

            var magnitude = math.length(velocity);

            velocity += moveBody.ForwardImpulseForce * moveInput;
            moveBody.ForwardImpulseForce = 0;

            if (moveInfo.softMaxSpeed != 0 && magnitude < moveInfo.softMaxSpeed)
            {
                velocity += moveInput * (deltaTime * moveInfo.accelerationSpeed);
            }
            else if (moveInfo.hardMaxSpeed != 0 && magnitude > moveInfo.hardMaxSpeed)
            {
                velocity = math.normalize(velocity) * moveInfo.hardMaxSpeed;
            }

            velocity -= math.normalizesafe(velocity, float2.zero) * (deltaTime * moveInfo.decelerationSpeed);

            moveBody.physicBodyAspect.Velocity = velocity;

            transform.Position.xy += velocity * (deltaTime);
        }
    }

    public struct MovePhysicsBody : IComponentData, IEnableableComponent
    {
        public float forwardImpulseForce;
        public float softMaxSpeed;
        public float hardMaxSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
    }

    public readonly partial struct PhysicsMoveBodyAspect : IAspect
    {
        public readonly PhysicsBodyAspect physicBodyAspect;
        private readonly RefRW<MovePhysicsBody> movePhysicsBody;

        public MovePhysicsBody MovePhysicsBody => movePhysicsBody.ValueRO;

        public float ForwardImpulseForce
        {
            get => movePhysicsBody.ValueRO.forwardImpulseForce;
            set => movePhysicsBody.ValueRW.forwardImpulseForce = value;
        }
    }
}