using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StatusEffectManager))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Strictly 3 Unity Units per second")] public float moveSpeed = 3f;
    [Tooltip("Optional jump force")] public float jumpForce = 5f;
    [Tooltip("Adjusts how slippery the little fellas are")] public float groundTraction = 15f;
    public float maxSlopeAngle = 45f;
    public LayerMask groundLayer;
    public float mouseSensitivity = 15f;

    [Header("References")]
    public Transform playerCamera;
    private StatusEffectManager _status;
    private Rigidbody _playerBody;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;
    private bool _isGrounded;



    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();

        _playerBody.freezeRotation = true;
        _playerBody.interpolation = RigidbodyInterpolation.None;  // why?
        _status = GetComponent<StatusEffectManager>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        _moveInput = moveAction.ReadValue<Vector2>();

        // Block jump when crystallized
        if (jumpAction.WasPressedThisFrame() && _isGrounded)
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

        _lookInput = lookAction.ReadValue<Vector2>();

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
        // 0. Crystallized: freeze horizontal, let gravity pull down
        if (_status != null && _status.IsCrystallized)
        {
            _playerBody.linearVelocity = new Vector3(0f, _playerBody.linearVelocity.y, 0f);
            return;
        }

        // 1. Declare move direction and speed
        Vector3 moveDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        float speed = moveSpeed;

        // 2. Fire: speed boost + random horizontal push
        if (_status != null && _status.IsOnFire)
        {
            speed *= _status.fireSpeedMultiplier;

            // Random horizontal push (world space)
            if (_status.fireRandomPush.sqrMagnitude > 0.01f)
            {
                moveDir += _status.fireRandomPush;
            }
        }

        // 3. Apply Slope Slowdown
        float slopeMultiplier = GetSlopeMultiplier();
        speed *= slopeMultiplier;

        // 4. Final velocity
        Vector3 targetVelocity = moveDir * speed;
        _playerBody.linearVelocity = new Vector3(targetVelocity.x,
                                                 _playerBody.linearVelocity.y,
                                                 targetVelocity.z);
    }

    private float GetSlopeMultiplier()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f, groundLayer))
        {
            float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
            if (slopeAngle > 0)
            {
                if (slopeAngle >= maxSlopeAngle) return 0f; // too steep
                return 1f - (slopeAngle / maxSlopeAngle);
            }
        }
        return 1f;
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
