using ECS.Components.Physics;
using ECS.Components.Shared;
using ECS.Systems.Enemy;
using ECS.Systems.Physics;
using ECS.Systems.Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using PhysicsBodyAspect = ECS.Components.Physics.PhysicsBodyAspect;
using Random = UnityEngine.Random;

namespace ECS.Components.Enemy
{
    public class EnemyAuthoring : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 1f;
        [SerializeField] private float softMaxSpeed = 2f;
        [SerializeField] private float hardMaxSpeed = 3f;
        [SerializeField] private float accelerationSpeed = 10f;
        [SerializeField] private float decelerationSpeed = 5f;
        [SerializeField] private float rotateSpeed = 2f;

        [SerializeField] private float fireDelay = .1f;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawn;

        public class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new EnemyInfo());
                AddComponent(entity, new Alive());
                AddComponent(entity, new PhysicsBody());
                AddComponent(entity, new MovePhysicsBody
                {
                    softMaxSpeed = authoring.softMaxSpeed,
                    hardMaxSpeed = authoring.hardMaxSpeed,
                    accelerationSpeed = authoring.accelerationSpeed,
                    decelerationSpeed = authoring.decelerationSpeed,
                });
                AddComponent(entity, new FaceDirection
                {
                    rotateSpeed = authoring.rotateSpeed,
                });
                AddComponent(entity, new Avoidance
                {
                    pushForce = 4f,
                    pushRadius = 2f,
                });
                if (authoring.projectilePrefab != null)
                {
                    AddComponent(entity, new Shooting
                    {
                        fireDelayTimer = Random.Range(0, authoring.fireDelay),
                        fireDelayDuration = authoring.fireDelay,

                        projectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
                        projectileSpawn = GetEntity(authoring.projectileSpawn, TransformUsageFlags.Dynamic),
                    });
                }
                AddComponent(entity, new Health
                {
                    autoDestroy = true,
                    currentHealth = authoring.maxHealth,
                    maxHealth = authoring.maxHealth,
                });
            }
        }
    }
    
    public struct EnemyInfo : IComponentData
    {
        public float2 targetPosition;
    }

    public readonly partial struct EnemyAspect : IAspect
    {
        private readonly RefRW<EnemyInfo> enemyInfo;
        public readonly PhysicsBodyAspect physicsBodyAspect;

        public EnemyInfo EnemyInfo
        {
            get => enemyInfo.ValueRO;
            set => enemyInfo.ValueRW = value;
        }

        public float2 TargetPosition
        {
            get => enemyInfo.ValueRO.targetPosition;
            set => enemyInfo.ValueRW.targetPosition = value;
        }
    }
}