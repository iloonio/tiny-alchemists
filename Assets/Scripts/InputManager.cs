using UnityEngine;
using UnityEngine.InputSystem;

//  InputManager.cs — All input actions defined in code

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Actions (read by PlayerMovement and PlayerInteraction)
    public InputAction MoveAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction InteractAction { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Define bindings in code
        MoveAction = new InputAction("Move", InputActionType.Value);
        MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        LookAction = new InputAction("Look", InputActionType.Value,
            binding: "<Mouse>/delta");

        JumpAction = new InputAction("Jump", InputActionType.Button,
            binding: "<Keyboard>/space");

        InteractAction = new InputAction("Interact", InputActionType.Button,
            binding: "<Mouse>/leftButton");
    }

    void OnEnable()
    {
        MoveAction.Enable();
        LookAction.Enable();
        JumpAction.Enable();
        InteractAction.Enable();
    }

    void OnDisable()
    {
        MoveAction.Disable();
        LookAction.Disable();
        JumpAction.Disable();
        InteractAction.Disable();
    }

    void OnDestroy()
    {
        MoveAction?.Dispose();
        LookAction?.Dispose();
        JumpAction?.Dispose();
        InteractAction?.Dispose();
    }
}
