#if UNITY_EDITOR

using Level;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Shared.Physics
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DebugCollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoxCollider>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out Boundary boundary))
            {
                DrawBox(boundary.boundingRect, Color.yellow);
            }

            foreach (var collider in SystemAPI.Query<BoxColliderAspect>())
            {   
                DrawBox(collider.GetWorldBounds(), Color.red);
            }
        }

        private static void DrawBox(float4 bounds, Color color)
        {
            var point1 = new Vector3(bounds.x-bounds.z, bounds.y-bounds.w);
            var point2 = new Vector3(bounds.x-bounds.z, bounds.y+bounds.w);
            var point3 = new Vector3(bounds.x+bounds.z, bounds.y+bounds.w);
            var point4 = new Vector3(bounds.x+bounds.z, bounds.y-bounds.w);

            Debug.DrawLine(point1, point2, color);
            Debug.DrawLine(point2, point3, color);
            Debug.DrawLine(point3, point4, color);
            Debug.DrawLine(point4, point1, color);
        }
    }
}

#endif