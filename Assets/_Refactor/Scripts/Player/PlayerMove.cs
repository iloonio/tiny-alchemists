using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _baseMoveAcceleration = 5f;
    [SerializeField] private float _baseMaxMoveSpeed = 5f;
    [SerializeField] private float _groundFriction = 10f;
    [SerializeField] private float _baseJumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundPoint;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private float _groundCheckDistance = 0.05f;
    [SerializeField] private float _maxGroundAngle = 45f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private RaycastHit _groundHit;
    private float _moveSpeedMultiplier = 1f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyFriction();
        Move();
    }

    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    private void OnJump()
    {
        Jump();
    }

    private void Move()
    {
        float horizontalSpeed = Vector3.Magnitude(new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z));
        if (horizontalSpeed > _baseMaxMoveSpeed * _moveSpeedMultiplier) return;

        Vector3 moveDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        float moveAcceleration = _baseMoveAcceleration * _moveSpeedMultiplier;

        if (IsGrounded())
        {
            moveDir = Vector3.ProjectOnPlane(moveDir, _groundHit.normal);
        }

        Vector3 acceleration = moveDir.normalized * moveAcceleration * Time.fixedDeltaTime;

        _rb.linearVelocity += acceleration;
    }

    private void ApplyFriction()
    {
        if (!IsGrounded()) return;

        float friction = _groundFriction * Time.fixedDeltaTime;

        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, Vector3.zero, friction);
    }

    private void Jump()
    {
        if (!IsGrounded()) return;
        _rb.AddForce(Vector3.up * _baseJumpForce, ForceMode.Impulse);
    }

    public bool IsMoving()
    {
        return _moveInput.sqrMagnitude > 0.01f;
    }

    public bool IsGrounded()
    {
        bool hit = Physics.SphereCast(_groundPoint.position, _groundCheckRadius, Vector3.down, out _groundHit, _groundCheckDistance, _groundLayer, QueryTriggerInteraction.Ignore);
        if (!hit) return false;

        float angle = Vector3.Angle(_groundHit.normal, Vector3.up);
        return angle <= _maxGroundAngle;
    }

    public void MultiplyMoveSpeed(float multiplier)
    {
        _moveSpeedMultiplier *= multiplier;
    }
}
