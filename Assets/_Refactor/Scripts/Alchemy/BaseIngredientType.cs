using UnityEngine;

public abstract class BaseIngredientType : IngredientType
{
    [SerializeField] private PotionEffect _effect;
    public PotionEffect Effect => _effect;
    public float Duration = 120f;

    public virtual void OnEffectSetup(PotionEffect effect) {}
    public virtual void OnEffectStart(PotionEffect effect) {}
    public virtual void OnEffectUpdate(PotionEffect effect) {}
    public virtual void OnEffectEnd(PotionEffect effect) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect) {}

}