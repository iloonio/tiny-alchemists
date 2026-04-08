using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Strictly 3 Unity Units per second")] public float moveSpeed = 3f;
    [Tooltip("Optional jump force")] public float jumpForce = 5f;
    public float mouseSensitivity = 15f;    

    [Header("References")]
    public Transform playerCamera;
    

    // Private fields for player stuff
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private Rigidbody _playerBody;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;
    private bool _isGrounded;

    // ���� Status Effect Integration ����
    private StatusEffectManager _status;

    // We need to enable specific actions for the player 
    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
        _status = GetComponent<StatusEffectManager>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // Update runs every frame. Do cheap operations here. 
    void Update()
    {
        _moveInput = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame() && _isGrounded)
        {
            _playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        HandleLook();
    }

    // FixedUpdate runs every fixed framerate frame. 
    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleLook()
    {
        // Read mouse delta (the movement since the last frame)
        if (playerCamera == null) return;
        _lookInput = lookAction.ReadValue<Vector2>();

        float lookX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = _lookInput.y * mouseSensitivity * Time.deltaTime;

        // ���� On-Fire jitter: add random offset to mouse look ����
        if (_status != null && _status.IsOnFire)
        {
            lookX += _status.cameraJitter.x * Time.deltaTime;
            lookY += _status.cameraJitter.y * Time.deltaTime;
        }

       // 1. Vertical Rotation (Up/Down) - Rotates the Camera only
        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 2. Horizontal Rotation (Left/Right) - Rotates the entire Player
        transform.Rotate(Vector3.up * lookX);
    }


    private void HandleMovement()
    {
        // ���� Crystallized: zero out horizontal velocity, let gravity pull down ����
        if (_status != null && _status.IsCrystallized)
        {
            _playerBody.linearVelocity = new Vector3(0f, _playerBody.linearVelocity.y, 0f);
            return;
        }
        
        Vector3 moveDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        // ���� On-Fire: sporadic forced forward movement ����
        if (_status != null && _status.sporadicForward > 0f)
        {
            moveDir += transform.forward * _status.sporadicForward;
        }

        Vector3 targetVelocity = moveDir * moveSpeed;

        _playerBody.linearVelocity = new Vector3(targetVelocity.x,
                                                 _playerBody.linearVelocity.y, 
                                                 targetVelocity.z);
    }

    private void OnCollisionStay(Collision collision)
    {
        _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }
}
