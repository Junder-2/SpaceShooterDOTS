using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Components.Physics
{
    public class CollisionBoxAuthoring : MonoBehaviour
    {
        [SerializeField] private EntityPhysics.LayerMask collisionLayer;
        [SerializeField] private Vector2 size;
        [SerializeField] private Vector2 offset;

        [SerializeField] private bool raiseCollisionEvents;
        [SerializeField] private bool raiseTriggerEvents;
        
        class Baker : Baker<CollisionBoxAuthoring>
        {
            public override void Bake(CollisionBoxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BoxCollider
                {
                    boundingRect = new float4(authoring.offset, authoring.size/2f),
                    layer = (byte)authoring.collisionLayer,
                    raiseCollisionEvents = authoring.raiseCollisionEvents,
                    raiseTriggerEvents = authoring.raiseTriggerEvents
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