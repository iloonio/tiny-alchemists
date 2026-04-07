using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementFPS : MonoBehaviour
{
    [Header("Movement (Based on Meeting Notes)")]
    [Tooltip("Strictly 3 Unity Units per second")]
    public float moveSpeed = 3f;

    [Tooltip("Optional jump force")]
    public float jumpForce = 5f;

    [Header("First Person Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 15f;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _xRotation = 0f;
    private bool _isGrounded;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        _rb.freezeRotation = true;

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    public void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext ctx) => _lookInput = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        
        if (ctx.performed && _isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Update()
    {
        
        HandleLook();
    }

    void FixedUpdate()
    {
        
        HandleMovement();
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        float lookX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = _lookInput.y * mouseSensitivity * Time.deltaTime;

       
        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); 
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        
        transform.Rotate(Vector3.up * lookX);
    }

    private void HandleMovement()
    {
        
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        
        Vector3 newVelocity = new Vector3(moveDir.x * moveSpeed, _rb.linearVelocity.y, moveDir.z * moveSpeed);

        _rb.linearVelocity = newVelocity;
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