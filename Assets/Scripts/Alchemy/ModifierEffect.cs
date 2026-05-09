using UnityEngine;
using System.Collections.Generic;

public abstract class ModifierEffect : IngredientEffect
{
    public virtual void OnEffectSetup(PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectSetup(effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectSetup(effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectSetup(effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectSetup(effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectStart(PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectStart(effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectStart(effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectStart(effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectStart(effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectUpdate(PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectUpdate(effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectUpdate(effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectUpdate(effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectUpdate(effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectEnd(PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectEnd(effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectEnd(effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectEnd(effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectEnd(effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectTriggerEnter(other, effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectTriggerEnter(other, effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectTriggerEnter(other, effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectTriggerEnter(other, effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, BaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (baseEffect is NoBaseEffect noBase)
            OnEffectTriggerExit(other, effect, noBase, modifierEffects);
        else if (baseEffect is CloudBaseEffect cloudBase)
            OnEffectTriggerExit(other, effect, cloudBase, modifierEffects);
        else if (baseEffect is CubeBaseEffect cubeBase)
            OnEffectTriggerExit(other, effect, cubeBase, modifierEffects);
        else if (baseEffect is PuddleBaseEffect puddleBase)
            OnEffectTriggerExit(other, effect, puddleBase, modifierEffects);
    }

    public virtual void OnEffectSetup(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectSetup(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectSetup(PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectSetup(PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

    public virtual void OnEffectStart(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectStart(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectStart(PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectStart(PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

    public virtual void OnEffectUpdate(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectUpdate(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectUpdate(PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectUpdate(PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

    public virtual void OnEffectEnd(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectEnd(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectEnd(PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectEnd(PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects) {}

}