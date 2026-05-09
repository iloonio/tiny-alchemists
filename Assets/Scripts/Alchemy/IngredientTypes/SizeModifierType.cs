using UnityEngine;

[CreateAssetMenu(fileName = "SizeModifier", menuName = "ScriptableObjects/IngredientType/SizeModifier")]
public class SizeModifierIngredientType : ModifierIngredientType
{
    [SerializeField] private float _scaleMultiplier = 1.5f;
    public float ScaleMultiplier => _scaleMultiplier;

    public override IngredientEffect CreateEffect()
    {
        return new SizeModifierEffect(ScaleMultiplier);
    }
}