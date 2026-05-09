using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLook : MonoBehaviour
{

    [Header("Look")]
    [SerializeField] private float _lookSensitivity = 20f;
    [SerializeField] private float _maxVerticalLookAngle = 80f;
    [SerializeField] private Transform _camera;

    private Vector2 _lookInput;
    private float _pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
    }

    private void OnLook(InputValue value)
    {
        _lookInput = value.Get<Vector2>();
    }

    private void Look()
    {
        Vector2 look = _lookInput * _lookSensitivity * Time.deltaTime;

        _pitch -= look.y;
        _pitch = Mathf.Clamp(_pitch, -_maxVerticalLookAngle, _maxVerticalLookAngle);
        _camera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        transform.Rotate(Vector3.up * look.x);
    }

    
}
