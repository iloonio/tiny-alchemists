using UnityEngine;

[CreateAssetMenu(fileName = "SparkleModifier", menuName = "ScriptableObjects/IngredientType/SparkleModifier")]
public class SparkleModifierIngredientType : ModifierIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new SparkleModifierEffect();
    }
}