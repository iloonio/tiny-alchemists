using UnityEngine;

[CreateAssetMenu(fileName = "PuddleBase", menuName = "ScriptableObjects/IngredientType/PuddleBase")]
public class PuddleBaseIngredientType : BaseIngredientType
{
    public override IngredientEffect CreateEffect()
    {
        return new PuddleBaseEffect(Duration);
    }
}