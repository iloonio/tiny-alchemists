using UnityEngine;

[CreateAssetMenu(fileName = "CloudBase", menuName = "ScriptableObjects/IngredientType/CloudBase")]
public class CloudBaseIngredientType : BaseIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new CloudBaseEffect(Duration);
    }
}