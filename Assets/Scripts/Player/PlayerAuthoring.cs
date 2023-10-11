using System.Collections;
using System.Collections.Generic;
using Generic;
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
                AddComponent(entity, new Player
                {
                    maxMoveSpeed = authoring.maxMoveSpeed,
                    accelerationSpeed = authoring.accelerationSpeed,
                    decelerationSpeed = authoring.decelerationSpeed,
                    input = new PlayerInput(),
                });
                AddComponent(entity, new FaceDirection
                {
                    direction = float2.zero,
                    rotateSpeed = authoring.rotateSpeed,
                });
            }
        }
    }

    public struct Player : IComponentData
    {
        public float maxMoveSpeed;
        public float accelerationSpeed;
        public float decelerationSpeed;
        
        public float2 velocity;
        public PlayerInput input;
    }

    public struct PlayerInput : IComponentData
    {
        public float2 movement;
        public float2 lookDir;
        public float2 mousePos;
        public float3 mouseWorldPos;
    }
}