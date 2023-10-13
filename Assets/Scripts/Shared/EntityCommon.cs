using Unity.Entities;
using Unity.Mathematics;

namespace Shared
{
    public struct EntityData : IComponentData
    {
        public float2 moveDirection;
        public float2 faceDirection;
    }

    public struct PhysicsData : IComponentData
    {
        public float2 velocity;
    }
}