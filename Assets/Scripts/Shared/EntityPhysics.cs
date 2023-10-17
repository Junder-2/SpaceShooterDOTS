using Unity.Entities;
using Unity.Mathematics;

namespace Shared
{
    public struct PhysicsBody : IComponentData
    {
        public float2 velocity;

        public float2 moveVector;
        public float2 faceDirection;
    }

    public readonly partial struct PhysicsBodyAspect : IAspect
    {
        private readonly RefRW<PhysicsBody> physicsBody;
        
        public PhysicsBody Body
        {
            get => physicsBody.ValueRO;
            set => physicsBody.ValueRW = value;
        }

        public float2 Velocity
        {
            get => physicsBody.ValueRO.velocity;
            set => physicsBody.ValueRW.velocity = value;
        }

        public float2 MoveVector
        {
            get => physicsBody.ValueRO.moveVector;
            set => physicsBody.ValueRW.moveVector = value;
        }
        
        public float2 FaceDirection
        {
            get => physicsBody.ValueRO.faceDirection;
            set => physicsBody.ValueRW.faceDirection = value;
        }
    } 
}