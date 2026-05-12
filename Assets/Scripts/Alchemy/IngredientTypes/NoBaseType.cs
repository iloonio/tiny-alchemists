using UnityEngine;

[CreateAssetMenu(fileName = "NoBase", menuName = "ScriptableObjects/IngredientType/NoBase")]
public class NoBaseIngredientType : BaseIngredientType
{
    [SerializeField] private float _radius = 1f;
    public float Radius => _radius;
    [SerializeField] private float _explosionForce = 5f;
    public float ExplosionForce => _explosionForce;

    public override IngredientEffect CreateEffect() {
        return new NoBaseEffect(Duration, Radius, ExplosionForce);
    }
}