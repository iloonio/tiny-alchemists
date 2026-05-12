using UnityEngine;

public abstract class BaseEffect : IngredientEffect
{
    public float Duration;
    public BaseEffect(float duration)
    {
        Duration = duration;
    }
    
    public virtual void OnEffectSetup(PotionEffect effect) {}
    public virtual void OnEffectStart(PotionEffect effect) {}
    public virtual void OnEffectUpdate(PotionEffect effect) {}
    public virtual void OnEffectEnd(PotionEffect effect) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect) {}
}