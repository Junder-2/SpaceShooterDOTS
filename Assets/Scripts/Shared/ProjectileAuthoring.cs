using Unity.Entities;
using UnityEngine;

namespace Shared
{
    public class ProjectileAuthoring : MonoBehaviour
    {
        [SerializeField] private float projectileMaxSpeed = 10;
        [SerializeField] private float projectileAcceleration = 20;
        [SerializeField] private float projectileDeceleration = 2;
        [SerializeField] private float projectileDamage = 1;

        class Baker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Projectile
                {
                    damage = authoring.projectileDamage,
                    lifeTimer = 0,
                    lifeTimeDuration = 5f
                });
                AddComponent(entity, new PhysicsBody());
                AddComponent(entity, new MovePhysicsBody
                {
                    softMaxSpeed = authoring.projectileMaxSpeed,
                    accelerationSpeed = authoring.projectileAcceleration,
                    decelerationSpeed = authoring.projectileDeceleration
                });
            }
        }
    }
}