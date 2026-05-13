using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : NetworkBehaviour
{
    private Rigidbody _rb;
    private Transform _holdPoint;
    private bool _isHeld = false;
    public bool IsHeld => _isHeld;

    private bool _savedGravity;
    private float _savedLinearDamping;
    private float _savedAngularDamping;
    private RigidbodyInterpolation _savedInterpolation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holdPoint, Transform camera = null, float collisionRadius = 0.25f)
    {
        if (IsHeld) return;

        _isHeld = true;
        _holdPoint = holdPoint;

        _savedGravity = _rb.useGravity;
        _savedAngularDamping = _rb.angularDamping;
        _savedLinearDamping = _rb.linearDamping;
        _savedInterpolation = _rb.interpolation;

        _rb.useGravity = false;
        _rb.linearDamping = 10f;
        _rb.angularDamping = 10f;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Drop()
    {
        if (!IsHeld) return;

        _isHeld = false;
        _rb.useGravity = _savedGravity;
        _rb.linearDamping = _savedLinearDamping;
        _rb.angularDamping = _savedAngularDamping;
        _rb.interpolation = _savedInterpolation;
    }

    public void Toss(Vector3 force)
    {
        Drop();
        _rb.AddForce(force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (!_isHeld || _holdPoint == null) return;

        _rb.MovePosition(Vector3.Lerp(_rb.position, _holdPoint.position, 20f * Time.fixedDeltaTime));

        Quaternion targetRot = Quaternion.LookRotation(_holdPoint.forward, Vector3.up);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 20f * Time.fixedDeltaTime));
    }
}