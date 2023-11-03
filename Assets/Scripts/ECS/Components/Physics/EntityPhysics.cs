using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Components.Physics
{
    public static class EntityPhysics
    {
        public const byte DefaultLayer = 1 << 1;
        public const byte PlayerLayer = 1 << 2;
        public const byte EnemyLayer = 1 << 3;
        public const byte ProjectileLayer = 1 << 4;
        public const byte BoundaryLayer = 1 << 5;
        public const byte BlockingLayer = 1 << 6;
        
        public const int CellSize = 2;
        public const int CellYOffset = 16;
        
        public static readonly float2[] CellOffsets =
        {
            new(-CellSize, CellSize), new(0, CellSize), new(CellSize, CellSize),
            new(-CellSize, 0), new(0, 0), new(CellSize, 0),
            new(-CellSize, -CellSize), new(0, -CellSize), new(-CellSize, -CellSize),
        };
        
        [Flags]
        public enum LayerMask : byte
        {
            None = 0,
            Default = DefaultLayer,
            Player = PlayerLayer,
            Enemy = EnemyLayer,
            Projectile = ProjectileLayer,
            Boundary = BoundaryLayer,
            Blocking = BlockingLayer,
        }
        
        public static bool CheckLayer(this CollisionMask collisionMask, byte layer)
        {
            return (collisionMask.includeLayers & layer) != 0 && (collisionMask.rejectLayers & layer) == 0;
        }

        public static bool AABBOverlap(float4 bounds, float4 otherBounds)
        {
            var dx = otherBounds.x - bounds.x;
            var px = (otherBounds.z + bounds.z) - math.abs(dx);
            if (px <= 0) return false;

            var dy = otherBounds.y - bounds.y;
            var py = (otherBounds.w + bounds.w) - math.abs(dy);
            if (py <= 0) return false;

            return true;
        }
        
        public static int GetSpatialHashMapKey(float2 pos)
        {
            return (int)(math.floor(pos.x / CellSize) + (math.floor(pos.y / CellSize) * CellYOffset));
        }

        public static float4 GetSpatialBounds(float2 pos)
        {
            var halfSize = CellSize / 2f;
            return new float4(math.floor(pos.x / CellSize) * CellSize + halfSize,
                math.floor(pos.y / CellSize) * CellSize + halfSize, halfSize, halfSize);
        }
    }

    public struct CollisionMask
    {
        public byte includeLayers;
        public byte rejectLayers;

        public CollisionMask(byte includeLayers, byte rejectLayers)
        {
            this.includeLayers = includeLayers;
            this.rejectLayers = rejectLayers;
        }

        public CollisionMask(EntityPhysics.LayerMask includeLayers, EntityPhysics.LayerMask rejectLayers)
        {
            this.includeLayers = (byte)includeLayers;
            this.rejectLayers = (byte)rejectLayers;
        }
    }
    
    public struct PhysicsBody : IComponentData
    {
        public float2 velocity;

        public float2 moveVector;
        public float2 faceDirection;
    }

    public readonly partial struct PhysicsBodyAspect : IAspect
    {
        private readonly RefRW<PhysicsBody> physicsBody;
        
        public PhysicsBody Body
        {
            get => physicsBody.ValueRO;
            set => physicsBody.ValueRW = value;
        }

        public float2 Velocity
        {
            get => physicsBody.ValueRO.velocity;
            set => physicsBody.ValueRW.velocity = value;
        }

        public float2 MoveVector
        {
            get => physicsBody.ValueRO.moveVector;
            set => physicsBody.ValueRW.moveVector = value;
        }
        
        public float2 FaceDirection
        {
            get => physicsBody.ValueRO.faceDirection;
            set => physicsBody.ValueRW.faceDirection = value;
        }
    }
    
    public struct BoxCollider : IComponentData
    {
        public bool raiseCollisionEvents;
        public bool raiseTriggerEvents;
        
        public byte layer;

        /// <summary>
        /// xy = offset, zw = halfWidth
        /// </summary>
        public float4 boundingRect;
    }

    public readonly partial struct BoxColliderAspect : IAspect
    {
        private readonly RefRO<BoxCollider> boxCollider;
        private readonly RefRO<LocalToWorld> localToWorld;

        public BoxCollider BoxCollider => boxCollider.ValueRO;
        public LocalToWorld LocalToWorld => localToWorld.ValueRO;

        public float4 GetBounds() => boxCollider.ValueRO.boundingRect;

        public float4 GetWorldBounds()
        {
            var pos = localToWorld.ValueRO.Position.xy;
            return boxCollider.ValueRO.boundingRect + new float4(pos.x, pos.y, 0, 0);
        }

        public bool CheckCollision(float4 otherWorldBounds)
        {
            var bounds = GetWorldBounds();

            return EntityPhysics.AABBOverlap(bounds, otherWorldBounds);
        }

        public bool CheckInverseCollision(float4 otherWorldBounds)
        {
            var bounds = GetWorldBounds();

            bounds.zw *= -1f;

            return !EntityPhysics.AABBOverlap(bounds, otherWorldBounds);
        }
    }

    public struct ColliderData
    {
        public BoxCollider boxCollider;
        public float4 worldBounds;
        public Entity entity;
    }
}
