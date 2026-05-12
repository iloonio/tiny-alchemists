using UnityEngine;

[CreateAssetMenu(fileName = "FireModifier", menuName = "ScriptableObjects/IngredientType/FireModifier")]
public class FireModifierIngredientType : ModifierIngredientType
{
    [SerializeField] private Status _fireStatus;
    public Status FireStatus => _fireStatus;
    [SerializeField] private float _fireStatusDuration = 5f;
    public float FireStatusDuration => _fireStatusDuration;
    public override IngredientEffect CreateEffect()
    {
        return new FireModifierEffect(FireStatus, FireStatusDuration);
    }
}