#if UNITY_EDITOR

using Level;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

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
            
            return;

            foreach (var player in SystemAPI.Query<Player.PlayerInput>())
            {
                DebugSpatialMap(player.mouseWorldPos.xy);
            }
        }
        
        private static void DebugSpatialMap(float2 pos)
        {
            var cellSize = EntityPhysics.CellSize;
            
            int key = (int)(math.floor(pos.x / cellSize) + (math.floor(pos.y / cellSize) * EntityPhysics.CellYOffset));
            
            DrawBox(new float4(math.floor(pos.x/cellSize)*cellSize + cellSize/2f, math.floor(pos.y/cellSize)*cellSize + cellSize/2f
                , cellSize/2f, cellSize/2f), Color.green);
            Debug.Log(key);
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