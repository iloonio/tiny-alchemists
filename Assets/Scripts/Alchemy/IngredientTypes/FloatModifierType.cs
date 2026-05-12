using UnityEngine;

[CreateAssetMenu(fileName = "FloatModifier", menuName = "ScriptableObjects/IngredientType/FloatModifier")]
public class FloatModifierIngredientType : ModifierIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new FloatModifierEffect();
    }
}