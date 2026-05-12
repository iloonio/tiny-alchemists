using UnityEngine;

[CreateAssetMenu(fileName = "MagneticModifier", menuName = "ScriptableObjects/IngredientType/MagneticModifier")]
public class MagneticModifierIngredientType : ModifierIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new MagneticModifierEffect();
    }
}