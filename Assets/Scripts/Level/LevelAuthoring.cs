using System;
using Shared.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Level
{
    [Serializable]
    public struct EnemyPrefab
    {
        public GameObject prefab;
        public int requiredDifficulty;
    }
    
    public class LevelAuthoring : MonoBehaviour
    {
        [SerializeField] private Vector2 boundarySize = new Vector2(20, 30);
        [SerializeField] private Vector2 boundaryOffset;

        class Baker : Baker<LevelAuthoring>
        {
            public override void Bake(LevelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                Vector2 pos = authoring.transform.position;
                Vector2 size = authoring.boundarySize;
                Vector2 offset = authoring.boundaryOffset;
                
                AddComponent(entity, new Boundary
                {
                    boundingRect = new float4(pos + offset, size/2f),
                    collisionLayer = EntityPhysics.BoundaryLayer,
                });
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)boundaryOffset, boundarySize);
        }
    }

    public struct Boundary : IComponentData
    {
        /// <summary>
        /// xy = offset, zw = halfWidth
        /// </summary>
        public float4 boundingRect;
        public byte collisionLayer;
    }
    
}