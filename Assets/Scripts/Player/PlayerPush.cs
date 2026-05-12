using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPush : MonoBehaviour
{
    [SerializeField] private float _pushForce = 8f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionStay(Collision collision)
    {
        Pushable pushable = collision.gameObject.GetComponent<Pushable>();
        if (pushable == null) return;

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
}