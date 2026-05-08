using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        Pushable pushable = collision.gameObject.GetComponent<Pushable>();

        if (pushable == null) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            pushable.PushServerRpc(-collision.impulse, contact.point);
        }
    }
}