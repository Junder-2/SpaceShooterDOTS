using ECS.Components.Level;
using ECS.Components.Physics;
using ECS.Systems.Physics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Level
{
    [UpdateAfter(typeof(CollisionDetectionSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct BoundarySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Boundary>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var boundary = SystemAPI.GetSingleton<Boundary>();
            var bounds = boundary.boundingRect;

            foreach (var (collisionEvent, collider, transform) 
                     in SystemAPI.Query<CollisionEvent, BoxColliderAspect, RefRW<LocalTransform>>().WithDisabled<CollisionEvent>())
            {
                if(!collisionEvent.isBoundary) continue;
                
                var otherBounds = collider.GetBounds();

                var pos = transform.ValueRO.Position;

                var clampX = bounds.x + bounds.z + otherBounds.x;
                var clampY = bounds.y + bounds.w + otherBounds.y;

                pos.x = math.clamp(pos.x, -clampX + otherBounds.z, clampX - otherBounds.z);
                pos.y = math.clamp(pos.y, -clampY + otherBounds.w, clampY - otherBounds.w);

                transform.ValueRW.Position = pos;
            }
        }
    }
}