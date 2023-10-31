﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Level
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private EnemyPrefab enemyPrefab;
        [SerializeField] private Vector2 spawnRadii = new Vector2(5f, 5f);

        class Baker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponentObject(entity, new EnemySpawner
                {
                    prefab = GetEntity(authoring.enemyPrefab.prefab, TransformUsageFlags.Dynamic),
                    requiredDifficulty = authoring.enemyPrefab.requiredDifficulty,
                    worldPosition = (Vector2)authoring.transform.position,
                    spawnRadii = authoring.spawnRadii
                });
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            
            Gizmos.DrawWireSphere(transform.position, spawnRadii.y);
        }
    }
    
    public class EnemySpawner : IComponentData
    {
        public Entity prefab;
        public int requiredDifficulty;

        public float2 worldPosition;
        public float2 spawnRadii;
    }
}