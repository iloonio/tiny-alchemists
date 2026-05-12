using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : NetworkBehaviour
{
    private Rigidbody _rb;
    private Transform _holdPoint;
    private Transform _camera;
    private float _collisionRadius = 0.25f;
    private bool _isHeld = false;
    public bool IsHeld => _isHeld;

    private float _savedLinearDamping;
    private float _savedAngularDamping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holdPoint, Transform camera = null, float collisionRadius = 0.25f)
    {
        if (IsHeld) return;

        _isHeld = true;
        _holdPoint = holdPoint;
        _camera = camera;
        _collisionRadius = collisionRadius;

        _savedAngularDamping = _rb.angularDamping;
        _savedLinearDamping = _rb.linearDamping;

        _rb.isKinematic = true;
    }

    public void Drop()
    {
        if (!IsHeld) return;

        _isHeld = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.linearDamping = _savedLinearDamping;
        _rb.angularDamping = _savedAngularDamping;
    }

    public void Toss(Vector3 force)
    {
        Drop();
        _rb.AddForce(force, ForceMode.Impulse);
    }

    private void LateUpdate()
    {
        if (!_isHeld || _holdPoint == null) return;

        Vector3 targetPos = _holdPoint.position;

        // Wall avoidance
        if (_camera != null)
        {
            Vector3 camPos = _camera.position;
            Vector3 toTarget = targetPos - camPos;
            float dist = toTarget.magnitude;

            if (dist > 0.01f && Physics.SphereCast(camPos, _collisionRadius,
                    toTarget.normalized, out RaycastHit wallHit, dist,
                    ~0, QueryTriggerInteraction.Ignore))
            {
                if (wallHit.collider.gameObject != gameObject)
                {
                    float safeDist = Mathf.Max(wallHit.distance - _collisionRadius, 0.1f);
                    targetPos = camPos + toTarget.normalized * safeDist;
                }
            }
        }

        transform.position = targetPos;
        transform.rotation = Quaternion.LookRotation(_holdPoint.forward, Vector3.up);
    }
}
