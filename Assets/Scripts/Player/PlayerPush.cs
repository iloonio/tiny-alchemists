using UnityEngine;
using Unity.Netcode;

// This file handles all physics involving players, which includes players pushing & being pushed by objects.
[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : NetworkBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<Pushable>(out var pushable))
        {
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            pushable.PushServerRpc(-collision.impulse, contact.point);
        }
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplyForceToPlayerClientRpc(float ExplosionForce, Vector3 ForceOrigin, float ExplosionRadius)
    {
        // THIS SHOULD WORK?
        gameObject.GetComponent<Rigidbody>().AddExplosionForce(ExplosionForce, ForceOrigin, ExplosionRadius, 1f, ForceMode.Impulse);
        Debug.Log($"Applied {ExplosionForce} force to player on the client-side from the point: {ForceOrigin}");
    }
}
