using UnityEngine;

[CreateAssetMenu(fileName = "BouncyModifier", menuName = "ScriptableObjects/IngredientType/BouncyModifier")]
public class BouncyModifierIngredientType : ModifierIngredientType
{
    [SerializeField] private Status _bouncyStatus;
    [SerializeField] private float _statusDuration = 5f;
    [SerializeField] private float _knockbackMultiplier = 2f;
    [SerializeField] private float _bounciness = 0.8f;

    public override IngredientEffect CreateEffect()
    {
        return new BouncyModifierEffect(_bouncyStatus, _statusDuration, _knockbackMultiplier, _bounciness);
    }
}