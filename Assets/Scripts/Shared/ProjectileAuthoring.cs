﻿using Shared.Physics;
using Unity.Entities;
using UnityEngine;

namespace Shared
{
    public class ProjectileAuthoring : MonoBehaviour
    {
        [SerializeField] private float projectileInitialForce = 2f;
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
                    lifeTimer = 0,
                    lifeTimeDuration = 5f
                });
                AddComponent(entity, new PhysicsBody());
                AddComponent(entity, new MovePhysicsBody
                {
                    forwardImpulseForce = authoring.projectileInitialForce,
                    softMaxSpeed = authoring.projectileMaxSpeed,
                    accelerationSpeed = authoring.projectileAcceleration,
                    decelerationSpeed = authoring.projectileDeceleration
                });
                AddComponent(entity, new DestroyOnCollision());
                
            }
        }
    }
}