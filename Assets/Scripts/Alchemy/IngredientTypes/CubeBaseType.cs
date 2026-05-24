using UnityEngine;

[CreateAssetMenu(fileName = "CubeBase", menuName = "ScriptableObjects/IngredientType/CubeBase")]
public class CubeBaseIngredientType : BaseIngredientType
{
    [SerializeField] private float _auraRadius = 0.5f;
    public float AuraRadius => _auraRadius;
    public override IngredientEffect CreateEffect()
    {
        return new CubeBaseEffect(Duration, AuraRadius);
    }
}