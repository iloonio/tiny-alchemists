using UnityEngine;
using Unity.Netcode;

public class NoBaseEffect : BaseEffect
{
    public float Radius;
    public float ExplosionForce;

    public NoBaseEffect(float duration, float radius, float explosionForce) : base(duration)
    {
        Radius = radius;
        ExplosionForce = explosionForce;
    }

    public override void OnEffectStart(PotionEffect effect)
    {
        foreach (var collider in Physics.OverlapSphere(effect.transform.position, Radius))
        {
            // Check to see if we hit a player first, if its owned by the server, we don't need to go through the ClientRpc I think?
            if (collider.CompareTag("Player") && !collider.GetComponent<NetworkObject>().IsOwnedByServer)
            {
                collider.GetComponent<PlayerPush>().ApplyForceToPlayerClientRpc(ExplosionForce, effect.transform.position, Radius);
            }
            else if (collider.TryGetComponent(out Rigidbody rb)) // Then handle server Authoritative parts
            {
                rb.AddExplosionForce(ExplosionForce, effect.transform.position, Radius, 1f, ForceMode.Impulse);
            }
        }
    }
}
