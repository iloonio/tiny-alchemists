using UnityEngine;

[CreateAssetMenu(fileName = "BouncyModifier", menuName = "ScriptableObjects/IngredientType/BouncyModifier")]
public class BouncyModifierIngredientType : ModifierIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new BouncyModifierEffect();
    }
}