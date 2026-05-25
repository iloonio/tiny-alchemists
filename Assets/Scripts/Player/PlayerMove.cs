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
    private NetworkClient _networkClient;
    private AudioPlayer _audioPlayer;
    private Transform _model;
    private Vector2 _moveInput;
    private RaycastHit _groundHit;
    private Vector3 _platformVelocity; // velocity sampled from a MovingPlatform when standing on one
    private Transform _currentPlatform;
    private bool _isInputOverridden = false;
    private Vector2 _inputOverride = Vector2.zero;
    private bool _moveAudioPlaying = false;

    [HideInInspector] public float MoveSpeedMultiplier = 1f;
    [HideInInspector] public float JumpForceMultiplier = 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _networkClient = GetComponent<NetworkClient>();
        _audioPlayer = GetComponentInChildren<AudioPlayer>();
        _model = GetComponentInChildren<Animator>().transform;
    }

    private void FixedUpdate()
    {
        _rb.rotation = transform.rotation;

        ApplyFriction();
        Move();

        // If standing on a moving platform, carry the player by moving it with the same displacement
        if (_currentPlatform != null && _platformVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 platformDisplacement = _platformVelocity * Time.fixedDeltaTime;
            platformDisplacement.y = 0f; // don't force vertical movement
            _rb.MovePosition(_rb.position + platformDisplacement);
        }

        ClampHorizontalSpeed();

        _networkClient.IsMoving.Value = IsMoving();
        _networkClient.IsGrounded.Value = IsGrounded(); 

        if (IsGrounded() && IsMoving() && !_moveAudioPlaying) {
            _audioPlayer.Play("Move");
            _model.transform.localPosition = -0.36f * Vector3.up; // TODO: remove once model is fixed
            _moveAudioPlaying = true;
        }
        else if ((!IsGrounded() || !IsMoving()) && _moveAudioPlaying) {
            _audioPlayer.Stop("Move");
            _model.transform.localPosition = -1.06f * Vector3.up;
            _moveAudioPlaying = false;
        }
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
        if (_isInputOverridden)
        {
            _moveInput = _inputOverride;
        } 

        float horizontalSpeed = Vector3.Magnitude(new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z));
        if (horizontalSpeed > _baseMaxMoveSpeed * MoveSpeedMultiplier) return;

        Vector3 moveDir = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        float moveAcceleration = _baseMoveAcceleration * MoveSpeedMultiplier;

        if (IsGrounded())
            moveDir = Vector3.ProjectOnPlane(moveDir, _groundHit.normal);

        Vector3 acceleration = moveDir.normalized * moveAcceleration * Time.fixedDeltaTime;
        _rb.linearVelocity += acceleration;
    }

    private void ClampHorizontalSpeed()
    {
        float maxSpeed = _baseMaxMoveSpeed * MoveSpeedMultiplier;
        Vector3 horizontal = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

        if (horizontal.magnitude > maxSpeed * 1.5f)
        {
            horizontal = horizontal.normalized * maxSpeed * 1.5f;
            _rb.linearVelocity = new Vector3(horizontal.x, _rb.linearVelocity.y, horizontal.z);
        }
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
        _audioPlayer.Play("Jump");
        _rb.AddForce(_baseJumpForce * JumpForceMultiplier * Vector3.up, ForceMode.Impulse);
    }

    public bool IsMoving() => (_isInputOverridden ? _inputOverride : _moveInput).sqrMagnitude > 0.0001f;

    /// IsGrounded checks for slopes and if we are standing on moving platforms using spherecasts.
    /// Returns false if it doesn't hit anything, or if the Angle of the sloped surface is greater than _maxGroundAngle.
    /// Returns true if it hits something and the surface's angle is less than the _maxGroundAngle.
    public bool IsGrounded()
    {
        bool hit = Physics.SphereCast(_groundPoint.position, _groundCheckRadius, Vector3.down, out _groundHit, _groundCheckDistance, _groundLayer, QueryTriggerInteraction.Ignore);
        if (!hit)
        {
            _currentPlatform = null;
            _platformVelocity = Vector3.zero;
            return false;
        }

        // check to see if we are standing on a moving platform and sample its velocity
        if (_groundHit.transform.gameObject.TryGetComponent(out MovingPlatform platform))
        {
            _currentPlatform = _groundHit.transform;
            _platformVelocity = platform.Velocity;
            // Debug.Log($"Standing on moving platform: {_currentPlatform.name} with velocity {_platformVelocity}");
        }
        else
        {
            _currentPlatform = null;
            _platformVelocity = Vector3.zero;
        }

        return Vector3.Angle(_groundHit.normal, Vector3.up) <= _maxGroundAngle;
    }

    public void OverrideInput(Vector2 input)
    {
        _inputOverride = input;
        _isInputOverridden = true;
    }

    public void ClearInputOverride()
    {
        _inputOverride = Vector2.zero;
        _isInputOverridden = false;
        _moveInput = Vector2.zero;
    }
}
