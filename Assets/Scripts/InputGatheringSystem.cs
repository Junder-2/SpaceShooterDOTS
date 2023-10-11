using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = Player.PlayerInput;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InputGatheringSystem : SystemBase, InputActions.IPlayerActions
{
    private EntityQuery playerInputQuery;
    
    private InputActions inputActions;
    private Vector2 moveInput;
    private Vector2 mousePos;
    private Vector2 lookInput;

    protected override void OnCreate()
    {
        inputActions = new InputActions();
        inputActions.Player.SetCallbacks(this);

        playerInputQuery = GetEntityQuery(typeof(PlayerInput));
    }

    protected override void OnStartRunning() => inputActions.Enable();
    protected override void OnStopRunning() => inputActions.Disable();

    protected override void OnUpdate()
    {
        if (playerInputQuery.CalculateEntityCount() == 0)
            EntityManager.CreateEntity(typeof(PlayerInput));

        Vector3 mouseWorld = Vector3.zero;
        mousePos = Vector2.zero;

        var cam = Camera.main;
        
        if (cam != null)
        {
            mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
            mouseWorld = cam.ViewportToWorldPoint(mousePos);
        }

        playerInputQuery.SetSingleton(new PlayerInput
        {
            movement = moveInput,
            lookDir = lookInput,
            mousePos = mousePos,
            mouseWorldPos = mouseWorld,
        });
    }
    
    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
}
