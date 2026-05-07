using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        PushableObject moveable = collision.gameObject.GetComponent<PushableObject>();

        if (moveable == null) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            moveable.PushServerRpc(-collision.impulse, contact.point);
        }
    }
}