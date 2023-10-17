using Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class EnemyAuthoring : MonoBehaviour
    {
        [SerializeField] private float softMaxSpeed = 2f;
        [SerializeField] private float hardMaxSpeed = 3f;
        [SerializeField] private float accelerationSpeed = 10f;
        [SerializeField] private float decelerationSpeed = 5f;
        [SerializeField] private float rotateSpeed = 2f;

        [SerializeField] private float fireDelay = .1f;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawn;

        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyInfo());
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

                AddComponent(entity, new Shooting
                {
                    fireDelayTimer = Random.Range(0, authoring.fireDelay),
                    fireDelayDuration = authoring.fireDelay,

                    projectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
                    projectileSpawn = GetEntity(authoring.projectileSpawn, TransformUsageFlags.Dynamic),
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