using Unity.Entities;
using UnityEngine;

namespace ECS.Components.Physics
{
    public class CollisionReceiverAuthoring : MonoBehaviour
    {
        [SerializeField] private EntityPhysics.LayerMask includeLayers;
        [SerializeField] private EntityPhysics.LayerMask rejectLayers;

        class Baker : Baker<CollisionReceiverAuthoring>
        {
            public override void Bake(CollisionReceiverAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CollisionEvent
                {
                    collisionMask = new CollisionMask(authoring.includeLayers, authoring.rejectLayers)
                });
            }
        }
    }
    
    
    /// <summary>
    /// true is no event false is event
    /// </summary>
    public struct CollisionEvent : IComponentData, IEnableableComponent
    {
        public CollisionMask collisionMask;
        
        public Entity collidedEntity;
        public bool isBoundary;
    }
}