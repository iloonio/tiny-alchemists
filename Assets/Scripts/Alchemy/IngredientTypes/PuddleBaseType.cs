using UnityEngine;

[CreateAssetMenu(fileName = "PuddleBase", menuName = "ScriptableObjects/IngredientType/PuddleBase")]
public class PuddleBaseIngredientType : BaseIngredientType
{
    [SerializeField] private float _auraRadius = 1f;
    public float AuraRadius => _auraRadius;

    public override IngredientEffect CreateEffect()
    {
        return new PuddleBaseEffect(Duration, AuraRadius);
    }
}