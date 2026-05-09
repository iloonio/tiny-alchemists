using UnityEngine;

[CreateAssetMenu(fileName = "CubeBase", menuName = "ScriptableObjects/IngredientType/CubeBase")]
public class CubeBaseIngredientType : BaseIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new CubeBaseEffect(Duration);
    }
}