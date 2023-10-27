using Shared.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Level
{
    public class LevelAuthoring : MonoBehaviour
    {
        class Baker : Baker<LevelAuthoring>
        {
            public override void Bake(LevelAuthoring authoring)
            {
                var levelManager = LevelManager.instance;
                var enemySpawnManager = EnemySpawnManager.instance;

                DependsOn(levelManager);
                DependsOn(enemySpawnManager);
                
                if(levelManager == null || enemySpawnManager == null) return;
                
                var entity = GetEntity(TransformUsageFlags.None);

                Vector2 pos = levelManager.transform.position;
                Vector2 size = levelManager.GetBoundarySize();
                Vector2 offset = levelManager.GetBoundaryOffset();
                
                AddComponent(entity, new LevelTag());
                AddComponent(entity, new Boundary
                {
                    boundingRect = new float4(pos + offset, size/2f),
                    collisionLayer = EntityPhysics.BoundaryLayer,
                });
                
                var enemyPrefabs = enemySpawnManager.GetEnemyPrefabs();
                
                if(enemyPrefabs == null) return;
                
                var entityBuffer = AddBuffer<EntityBufferElement>(entity);

                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    var newEntity = GetEntity(enemyPrefabs[i].prefab, TransformUsageFlags.Dynamic);
                    entityBuffer.Add(newEntity);
                }
            }
        }
    }

    public struct LevelTag : IComponentData
    {
        
    }
    
    public struct Boundary : IComponentData
    {
        /// <summary>
        /// xy = offset, zw = halfWidth
        /// </summary>
        public float4 boundingRect;
        public byte collisionLayer;
    }
    
    [InternalBufferCapacity(8)]
    public struct EntityBufferElement : IBufferElementData
    {
        public Entity entity;
        
        public static implicit operator Entity(EntityBufferElement e)
        {
            return e.entity;
        }

        public static implicit operator EntityBufferElement(Entity e)
        {
            return new EntityBufferElement { entity = e };
        }
    }

    public struct EnemySpawner : IComponentData, IEnableableComponent
    {
        public int prefabIndex;
        public int spawnAmount;

        public float2 worldPosition;
        public float spawnRadius;
    }
}