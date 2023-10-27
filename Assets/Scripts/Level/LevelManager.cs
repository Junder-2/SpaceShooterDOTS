using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;
        
        [SerializeField] private Vector2 boundarySize;
        [SerializeField] private Vector2 boundaryOffset;

        private void Awake()
        {
            instance = this;
        }

        public Vector2 GetBoundarySize() => boundarySize;
        public Vector2 GetBoundaryOffset() => boundaryOffset;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)boundaryOffset, boundarySize);
        }
    }
}