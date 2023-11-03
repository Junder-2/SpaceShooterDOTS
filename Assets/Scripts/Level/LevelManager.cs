using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    [DefaultExecutionOrder(-1)]
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance;

        private void Awake()
        {
            instance = this;
        }
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}