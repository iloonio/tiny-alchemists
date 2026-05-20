using UnityEngine;

[CreateAssetMenu(fileName = "FloatModifier", menuName = "ScriptableObjects/IngredientType/FloatModifier")]
public class FloatModifierIngredientType : ModifierIngredientType
{
    [SerializeField] private Status _floatStatus;
    [SerializeField] private float _statusDuration = 5f;
    [SerializeField] private float _cubeDrag = 5f;
    [SerializeField] private float _puddleUpwardForce = 15f;

    public override IngredientEffect CreateEffect()
    {
        return new FloatModifierEffect(_floatStatus, _statusDuration, _cubeDrag, _puddleUpwardForce);
    }
}