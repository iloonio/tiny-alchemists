using UnityEngine;

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
        Debug.Log("Yey");
        foreach (var collider in Physics.OverlapSphere(effect.transform.position, Radius))
        {
            if (collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(ExplosionForce, effect.transform.position, Radius, 1f, ForceMode.Impulse);
            }
        }
    }
}