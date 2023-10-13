using System.Collections;
using System.Collections.Generic;
using Shared;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Player
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] float maxMoveSpeed = 20f;
        [SerializeField] float accelerationSpeed = 10f;
        [SerializeField] float decelerationSpeed = 5f;
        [SerializeField] float rotateSpeed = 2f;

        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerInfo
                {
                    maxMoveSpeed = authoring.maxMoveSpeed,
                    accelerationSpeed = authoring.accelerationSpeed,
                    decelerationSpeed = authoring.decelerationSpeed,
                });
                AddComponent(entity, new EntityData());
                AddComponent(entity, new PlayerInput());
                AddComponent(entity, new PhysicsData());
                AddComponent(entity, new FaceDirection
                {
                    rotateSpeed = authoring.rotateSpeed,
                });
            }
        }
    }

    public struct PlayerInfo : IComponentData
    {
        public float maxMoveSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
    }

    public struct PlayerInput : IComponentData
    {
        public float2 movement;
        public float2 lookDir;
        public float2 mousePos;
        public float3 mouseWorldPos;
    }
    
    public readonly partial struct PlayerAspect : IAspect
    {
        private readonly RefRO<PlayerInfo> playerInfo;
        private readonly RefRO<PlayerInput> playerInput;
        private readonly RefRW<EntityData> entityData;
        private readonly RefRW<PhysicsData> physicsData;

        public PlayerInfo PlayerInfo => playerInfo.ValueRO;
        public PlayerInput InputData => playerInput.ValueRO;
        public EntityData EntityData
        {
            get => entityData.ValueRO;
            set => entityData.ValueRW = value;
        }

        public PhysicsData PhysicsData
        {
            get => physicsData.ValueRO; 
            set => physicsData.ValueRW = value;
        }
    }
}