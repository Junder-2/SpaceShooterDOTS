using ECS.Components.Physics;
using ECS.Components.Shared;
using ECS.Systems.Physics;
using ECS.Systems.Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Components.Player
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 3f;
        [SerializeField] private float softMaxSpeed = 2f;
        [SerializeField] private float hardMaxSpeed = 3f;
        [SerializeField] private float accelerationSpeed = 10f;
        [SerializeField] private float decelerationSpeed = 5f;
        [SerializeField] private float rotateSpeed = 2f;

        [SerializeField] private float fireDelay = .1f;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawn;

        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerInfo());
                AddComponent(entity, new PlayerInput());
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

                AddComponent(entity, new Shooting
                {
                    fireDelayTimer = 0,
                    fireDelayDuration = authoring.fireDelay,

                    projectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
                    projectileSpawn = GetEntity(authoring.projectileSpawn, TransformUsageFlags.Dynamic),
                });
                AddComponent(entity, new Health
                {
                    autoDestroy = false,
                    currentHealth = authoring.maxHealth,
                    maxHealth = authoring.maxHealth,
                });
            }
        }
    }

    public struct PlayerInfo : IComponentData
    {
        
    }

    public struct PlayerInput : IComponentData
    {
        public bool fire;
        public float2 movement;
        public float2 lookDir;
        public float2 mousePos;
        public float3 mouseWorldPos;
    }
    
    public readonly partial struct PlayerAspect : IAspect
    {
        private readonly RefRO<PlayerInfo> playerInfo;
        private readonly RefRO<PlayerInput> playerInput;
        private readonly RefRO<Health> health;
        public readonly PhysicsBodyAspect physicsBodyAspect;

        public PlayerInfo GetPlayerInfo() => playerInfo.ValueRO;
        public PlayerInput GetInputData() => playerInput.ValueRO;

        public Health GetHealth() => health.ValueRO;
    }
}