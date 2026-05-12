using UnityEngine;

public abstract class BaseIngredientType : IngredientType
{
    [SerializeField] private PotionEffect _potionEffectPrefab;
    public PotionEffect PotionEffectPrefab => _potionEffectPrefab;
    [SerializeField] private float _duration = 120f;
    public float Duration => _duration;
}