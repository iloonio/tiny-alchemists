using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StatusEffectManager))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Strictly 3 Unity Units per second")] public float moveSpeed = 3f;
    [Tooltip("Optional jump force")] public float jumpForce = 5f;
    public float mouseSensitivity = 15f;    

    [Header("References")]
    public Transform playerCamera;

    private Rigidbody _playerBody;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;
    private bool _isGrounded;

    private StatusEffectManager _status;

    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
        _playerBody.freezeRotation = true;      
        _playerBody.interpolation = RigidbodyInterpolation.None;  
        _status = GetComponent<StatusEffectManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    { 
        var input = InputManager.Instance;
        if (input == null) return;

        _moveInput = input.MoveAction.ReadValue<Vector2>();
        
        // Block jump when crystallized
        if (input.JumpAction.WasPressedThisFrame() && _isGrounded)
        {
            if (_status == null || !_status.IsCrystallized)
                _playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        HandleLook();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;
        var input = InputManager.Instance;
        if (input == null) return;

        _lookInput = input.LookAction.ReadValue<Vector2>();

        float lookX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = _lookInput.y * mouseSensitivity * Time.deltaTime;

        // Vertical rotation (camera only)
        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * lookX);
    }

    private void HandleMovement()
    {
        // Crystallized: freeze horizontal, let gravity pull down 
        if (_status != null && _status.IsCrystallized)
        {
            _playerBody.linearVelocity = new Vector3(0f, _playerBody.linearVelocity.y, 0f);
            return;
        }
        
        Vector3 moveDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        // Fire: speed boost + random horizontal push
        float speed = moveSpeed;
        if (_status != null && _status.IsOnFire)
        {
            speed *= _status.fireSpeedMultiplier;

            // Random horizontal push (world space)
            if (_status.fireRandomPush.sqrMagnitude > 0.01f)
            {
                moveDir += _status.fireRandomPush;
            }
        }

        Vector3 targetVelocity = moveDir * speed;
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
