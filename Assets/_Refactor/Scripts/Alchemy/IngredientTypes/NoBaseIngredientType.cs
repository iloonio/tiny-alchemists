using UnityEngine;

[CreateAssetMenu(fileName = "NoBase", menuName = "ScriptableObjects/IngredientType/NoBase")]
public class NoBaseIngredientType : BaseIngredientType
{
    public float Radius = 1f;
    public float ExplosionForce = 5f;

    public override void OnEffectStart(PotionEffect effect)
    {
        foreach (var collider in Physics.OverlapSphere(effect.transform.position, Radius))
        {
            if (collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(ExplosionForce, effect.transform.position, Radius, 1f, ForceMode.Impulse);
            }
        }
    }
}