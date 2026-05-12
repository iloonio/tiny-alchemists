using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : NetworkBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<Pushable>(out var pushable))
        {
            Debug.Log("No Pushable component found! How was this called?");
            return;
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            pushable.PushServerRpc(-collision.impulse, contact.point);
        }
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplyForceToPlayerClientRpc()
    {
        // Logic here
        Debug.Log("Applying force to player on client!");
    }
}
