using Unity.Entities;
using UnityEngine;

namespace Shared.Physics
{
    public class TriggerReceiverAuthoring : MonoBehaviour
    {
        [SerializeField] private EntityPhysics.LayerMask includeLayers;
        [SerializeField] private EntityPhysics.LayerMask rejectLayers;

        class Baker : Baker<TriggerReceiverAuthoring>
        {
            public override void Bake(TriggerReceiverAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TriggerEvent()
                {
                    collisionMask = new CollisionMask(authoring.includeLayers, authoring.rejectLayers)
                });
            }
        }
    }
    
    /// <summary>
    /// true is no event false is event
    /// </summary>
    public struct TriggerEvent : IComponentData, IEnableableComponent
    {
        public CollisionMask collisionMask;
        
        public Entity collidedEntity;
    }
}