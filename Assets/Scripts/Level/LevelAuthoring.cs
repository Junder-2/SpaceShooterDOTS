using Shared.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Level
{
    public class LevelAuthoring : MonoBehaviour
    {
        [SerializeField] private Vector2 size;
        [SerializeField] private Vector2 offset;
        
        class Baker : Baker<LevelAuthoring>
        {
            public override void Bake(LevelAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                Vector2 pos = authoring.transform.position;
                
                AddComponent(entity, new Boundary
                {
                    boundingRect = new float4(pos + authoring.offset, authoring.size/2f),
                    collisionLayer = EntityPhysics.BoundaryLayer,
                });
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)offset, size);
        }
    }
}