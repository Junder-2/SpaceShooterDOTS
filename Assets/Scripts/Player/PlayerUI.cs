using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;

        private void OnEnable()
        {
            var PlayerPostProcess = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ECS.Systems.Player.PlayerPostProcessSystem>();

            PlayerPostProcess.OnUpdateHealth += UpdateHealthDisplay;
        }

        private void UpdateHealthDisplay(float current, float maxHealth)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.minValue = 0;
            healthSlider.value = current;
        }
    }
}
