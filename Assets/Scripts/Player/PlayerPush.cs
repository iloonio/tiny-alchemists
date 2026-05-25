using UnityEngine;
using Unity.Netcode;

// This file handles all physics involving players, which includes players pushing & being pushed by objects.
[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : NetworkBehaviour
{
    [SerializeField] private float _pushForce = 8f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<Pushable>(out var pushable))
        {
            return;
        }

        Vector3 pushDir = collision.transform.position - transform.position;
        pushDir.y = 0f;
        pushDir.Normalize();

        if (pushDir.sqrMagnitude < 0.01f) return;

        float dot = Vector3.Dot(_rb.linearVelocity.normalized, pushDir);
        if (dot < 0.3f) return;

        if (collision.contactCount > 0)
        {
            pushable.PushServerRpc(pushDir * _pushForce, collision.GetContact(0).point);
        }
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddExplosionForceClientRpc(float explosionForce, Vector3 forceOrigin, float explosionRadius)
    {
        _rb.AddExplosionForce(explosionForce, forceOrigin, explosionRadius, 1f, ForceMode.Impulse);
        Debug.Log($"Applied {explosionForce} force to player on the client-side from the point: {forceOrigin}");
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddForceClientRpc(Vector3 force, ForceMode mode = ForceMode.Impulse)
    {
        _rb.AddForce(force, mode);
        Debug.Log($"[Cauldron] Launched player with force: {force}");
    }
}
