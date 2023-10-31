using UnityEngine;

namespace Level
{
    [DefaultExecutionOrder(-1)]
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager instance;

        public static LevelManager Instance
        {
            get { return instance ??= FindObjectOfType<LevelManager>(); }
        }

        private void Awake()
        {
            instance = this;
        }
    }
}