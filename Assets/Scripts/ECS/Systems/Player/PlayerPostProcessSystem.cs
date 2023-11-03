using System;
using ECS.Components.Player;
using Unity.Entities;
using PlayerAspect = ECS.Components.Player.PlayerAspect;

namespace ECS.Systems.Player
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class PlayerPostProcessSystem : SystemBase
    {
        public Action<float, float> OnUpdateHealth;

        private float lastHealth;

        protected override void OnCreate()
        {
            RequireForUpdate<PlayerInfo>();
        }

        protected override void OnUpdate()
        {
            foreach (var player in SystemAPI.Query<PlayerAspect>())
            {
                var health = player.GetHealth();

                if (health.currentHealth != lastHealth)
                {
                    lastHealth = health.currentHealth;
                    OnUpdateHealth?.Invoke(lastHealth, health.maxHealth);
                }
            }
        }
    }
}