using Unity.Entities;
using UnityEngine;

namespace Shared
{
    public class DamageAuthoring : MonoBehaviour
    {
        [SerializeField] private bool autoDestroy = true;
        [SerializeField] private float damage = .5f;
        
        public class Baker : Baker<DamageAuthoring>
        {
            public override void Bake(DamageAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new Damage
                {
                    autoDestroy = authoring.autoDestroy,
                    damageAmount = authoring.damage
                });
            }
        }
    }
}