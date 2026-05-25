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
            // Apply explosion force to affected players and objects
            if (collider.TryGetComponent(out PlayerPush playerPush))
            {
                playerPush.AddExplosionForceClientRpc(ExplosionForce, effect.transform.position, Radius);
            }
            else if (collider.TryGetComponent(out Rigidbody rb)) // Then handle server Authoritative parts
            {
                rb.AddExplosionForce(ExplosionForce, effect.transform.position, Radius, 1f, ForceMode.Impulse);
            }
        }
    }
}
