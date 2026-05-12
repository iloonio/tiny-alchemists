using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : NetworkBehaviour
{
    [SerializeField] private float _followPositionAcceleration = 50f;
    [SerializeField] private float _followRotationAcceleration = 30f;

    private Rigidbody _rb;
    private Transform _holdPoint;
    private bool _isHeld = false;
    public bool IsHeld => _isHeld;

    private float _savedLinearDamping;
    private float _savedAngularDamping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (IsHeld) return;

        _isHeld = true;
        _holdPoint = holdPoint;

        _savedAngularDamping = _rb.angularDamping;
        _savedLinearDamping = _rb.linearDamping;

        _rb.useGravity = false;
        _rb.linearDamping = 10f;
        _rb.angularDamping = 10f;
    }

    public void Drop()
    {
        if (!IsHeld) return;

        _isHeld = false;
        _rb.useGravity = true;
        _rb.linearDamping = _savedLinearDamping;
        _rb.angularDamping = _savedAngularDamping;
    }

    public void Toss(Vector3 force)
    {
        _rb.AddForce(force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (IsHeld)
        {
            Follow();
        }
    }

    public void Follow()
    {
        Vector3 toTarget = _holdPoint.position - _rb.position;

        Vector3 desiredVelocity = toTarget * _followPositionAcceleration;

        Vector3 velocityChange = desiredVelocity - _rb.linearVelocity;

        _rb.AddForce(velocityChange, ForceMode.Acceleration);

        Quaternion targetRotation = Quaternion.LookRotation(_holdPoint.forward, Vector3.up);

        Quaternion delta = targetRotation * Quaternion.Inverse(_rb.rotation);

        delta.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
            angle -= 360f;

        if (Mathf.Abs(angle) > 0.01f)
        {
            Vector3 angularVelocityTarget = axis * angle * Mathf.Deg2Rad * _followRotationAcceleration;

            Vector3 angularVelocityChange = angularVelocityTarget - _rb.angularVelocity;

            _rb.AddTorque(angularVelocityChange, ForceMode.Acceleration);
        }
    }

}

