using UnityEngine;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════════════════════════
//  PlayerMovementFPS.cs
//
//  FIXES:
//    1. Wall-stuck: Ground detection uses SphereCast downward
//       instead of OnCollisionStay (which triggered on walls)
//    2. Slopes: Movement is projected onto the slope surface
//       so the player doesn't slow down or bounce on ramps
//    3. Fire push: Applied as AddForce impulse instead of
//       directly changing moveDir (feels more natural)
// ═══════════════════════════════════════════════════════════════

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Strictly 3 Unity Units per second")]
    public float moveSpeed = 3f;
    [Tooltip("Optional jump force")]
    public float jumpForce = 5f;
    public float mouseSensitivity = 15f;

    [Header("Ground Detection")]
    [Tooltip("Extra distance below capsule to check for ground")]
    public float groundCheckDistance = 0.15f;
    [Tooltip("Max slope angle the player can walk on (degrees)")]
    public float maxSlopeAngle = 45f;

    [Header("References")]
    public Transform playerCamera;

    private Rigidbody _playerBody;
    private CapsuleCollider _capsule;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;

    // Ground / slope state
    private bool _isGrounded;
    private Vector3 _groundNormal = Vector3.up;
    private RaycastHit _groundHit;

    private StatusEffectManager _status;

    void Start()
    {
        _playerBody = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();
        _playerBody.freezeRotation = true;
        _playerBody.interpolation = RigidbodyInterpolation.Interpolate;
        //_playerBody.interpolation = RigidbodyInterpolation.None;
        _status = GetComponent<StatusEffectManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        var input = InputManager.Instance;
        if (input == null) return;

        _moveInput = input.MoveAction.ReadValue<Vector2>();

        // Jump: only when grounded and not crystallized
        if (input.JumpAction.WasPressedThisFrame() && _isGrounded)
        {
            if (_status == null || !_status.IsCrystallized)
            {
                // Reset vertical velocity before jumping for consistent height
                _playerBody.linearVelocity = new Vector3(
                    _playerBody.linearVelocity.x, 0f, _playerBody.linearVelocity.z);
                _playerBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        HandleLook();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
    }

    // ──────────────────────────────────────────────
    //  GROUND CHECK (SphereCast downward, not OnCollisionStay)
    // ──────────────────────────────────────────────

    private void CheckGround()
    {
        // SphereCast from center of capsule, downward
        float radius = _capsule.radius * 0.9f; // slightly smaller to avoid wall edges
        float halfHeight = _capsule.height * 0.5f;
        Vector3 origin = transform.position + _capsule.center;
        float castDistance = halfHeight - radius + groundCheckDistance;

        if (Physics.SphereCast(origin, radius, Vector3.down, out _groundHit, castDistance))
        {
            float slopeAngle = Vector3.Angle(_groundHit.normal, Vector3.up);
            _isGrounded = slopeAngle <= maxSlopeAngle;
            _groundNormal = _groundHit.normal;
        }
        else
        {
            _isGrounded = false;
            _groundNormal = Vector3.up;
        }
    }

    // ──────────────────────────────────────────────
    //  LOOK
    // ──────────────────────────────────────────────

    private void HandleLook()
    {
        if (playerCamera == null) return;
        var input = InputManager.Instance;
        if (input == null) return;

        _lookInput = input.LookAction.ReadValue<Vector2>();

        float lookX = _lookInput.x * mouseSensitivity * 0.01f;
        float lookY = _lookInput.y * mouseSensitivity * 0.01f;

        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookX);
    }

    // ──────────────────────────────────────────────
    //  MOVEMENT (slope-aware + fire as force)
    // ──────────────────────────────────────────────

    private void HandleMovement()
    {
        _playerBody.rotation = transform.rotation;

        // Crystallized: freeze horizontal
        if (_status != null && _status.IsCrystallized)
        {
            _playerBody.linearVelocity = new Vector3(0f, _playerBody.linearVelocity.y, 0f);
            return;
        }

        Vector3 inputDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        float speed = moveSpeed;

        // Fire: speed boost (multiplier only, push is separate)
        if (_status != null && _status.IsOnFire)
        {
            speed *= _status.fireSpeedMultiplier;
        }

        Vector3 moveDir;

        if (_isGrounded)
        {
            // Project movement onto slope surface
            moveDir = Vector3.ProjectOnPlane(inputDir, _groundNormal).normalized;
            // If no input, zero out (don't slide down gentle slopes)
            if (inputDir.sqrMagnitude < 0.01f)
                moveDir = Vector3.zero;
        }
        else
        {
            // In air: flat horizontal movement
            moveDir = inputDir;
        }

        Vector3 targetVel = moveDir * speed;

        // Preserve vertical velocity (gravity / jump)
        _playerBody.linearVelocity = new Vector3(targetVel.x, _playerBody.linearVelocity.y, targetVel.z);

        // Fire: random horizontal push as a FORCE (not direct velocity change)
        if (_status != null && _status.IsOnFire && _status.fireRandomPush.sqrMagnitude > 0.01f)
        {
            _playerBody.AddForce(_status.fireRandomPush, ForceMode.Impulse);
        }
    }
}
