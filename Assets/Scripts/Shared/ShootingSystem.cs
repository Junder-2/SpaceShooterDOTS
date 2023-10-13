using Unity.Burst;
using Unity.Entities;

namespace Shared
{
    public partial struct ShootingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }
    }
    
    public struct Shooting : IComponentData
    {
        public float rotateSpeed;
    }
}