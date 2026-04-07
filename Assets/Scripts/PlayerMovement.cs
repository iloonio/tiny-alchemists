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
        // 第一人称绝对不能让物理引擎帮你旋转
        _rb.freezeRotation = true;

        // 锁定并隐藏鼠标指针，FPS游戏必备
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ── Input System 回调 ──
    public void OnMove(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext ctx) => _lookInput = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // 只有按下瞬间且在地面上才能跳跃
        if (ctx.performed && _isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Update()
    {
        // 视角旋转放在 Update 中处理，以保证画面平滑
        HandleLook();
    }

    void FixedUpdate()
    {
        // 物理移动放在 FixedUpdate 中处理
        HandleMovement();
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        float lookX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = _lookInput.y * mouseSensitivity * Time.deltaTime;

        // 上下看（旋转相机本身）
        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // 限制抬头和低头的角度，防止脖子折断
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 左右看（旋转整个玩家身体）
        transform.Rotate(Vector3.up * lookX);
    }

    private void HandleMovement()
    {
        // 根据玩家当前的朝向来计算移动方向 (W是前进，A是左侧平移)
        Vector3 moveDir = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        // 保持 Y 轴速度（为了重力和跳跃），强制设置 X 和 Z 轴速度为 3 UU/sec
        Vector3 newVelocity = new Vector3(moveDir.x * moveSpeed, _rb.linearVelocity.y, moveDir.z * moveSpeed);

        _rb.linearVelocity = newVelocity;
    }

    // ── 简易的地面检测 (防止空中无限跳跃) ──
    private void OnCollisionStay(Collision collision)
    {
        // 只要碰到底部物体就认为在地面上
        _isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _isGrounded = false;
    }
}