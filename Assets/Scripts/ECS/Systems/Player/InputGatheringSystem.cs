using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = ECS.Components.Player.PlayerInput;

namespace ECS.Systems.Player
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InputGatheringSystem : SystemBase, InputActions.IPlayerActions
    {
        private InputActions inputActions;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool fireInput;
        
        protected override void OnCreate()
        {
            inputActions = new InputActions();
            inputActions.Player.SetCallbacks(this);
        }

        protected override void OnStartRunning() => inputActions.Enable();
        protected override void OnStopRunning() => inputActions.Disable();
        
        protected override void OnUpdate()
        {
            Vector3 mouseWorld = Vector3.zero;
            Vector2 mousePos = Vector2.zero;

            var cam = Camera.main;
        
            if (cam != null)
            {
                mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
                mouseWorld = cam.ViewportToWorldPoint(mousePos);
            }

            PlayerInput masterInput = new PlayerInput
            {
                fire = fireInput, lookDir = lookInput, movement = moveInput, mousePos = mousePos, mouseWorldPos = mouseWorld
            };

            Entities.ForEach((ref PlayerInput playerInput) => {
                playerInput.fire = masterInput.fire;
                playerInput.movement = masterInput.movement;
                playerInput.lookDir = masterInput.lookDir;
                playerInput.mousePos = masterInput.mousePos;
                playerInput.mouseWorldPos = masterInput.mouseWorldPos;
            }).ScheduleParallel();
        }
    
        public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
        public void OnFire(InputAction.CallbackContext context) => fireInput = context.ReadValueAsButton();
    }
}
