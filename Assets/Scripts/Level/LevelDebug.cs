using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    public class LevelDebug : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}