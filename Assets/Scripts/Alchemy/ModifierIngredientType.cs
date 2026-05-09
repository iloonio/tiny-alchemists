using System.Collections.Generic;
using UnityEngine;

public abstract class ModifierIngredientType : IngredientType
{
    public virtual void OnEffectSetup(PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectSetup(effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectSetup(effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectSetup(effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectSetup(effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectStart(PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectStart(effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectStart(effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectStart(effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectStart(effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectUpdate(PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectUpdate(effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectUpdate(effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectUpdate(effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectUpdate(effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectEnd(PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectEnd(effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectEnd(effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectEnd(effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectEnd(effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectTriggerEnter(other, effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectTriggerEnter(other, effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectTriggerEnter(other, effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectTriggerEnter(other, effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, BaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        if (baseIngredient is NoBaseIngredientType noBase)
            OnEffectTriggerExit(other, effect, noBase, modifierIngredients);
        else if (baseIngredient is CloudBaseIngredientType cloudBase)
            OnEffectTriggerExit(other, effect, cloudBase, modifierIngredients);
        else if (baseIngredient is CubeBaseIngredientType cubeBase)
            OnEffectTriggerExit(other, effect, cubeBase, modifierIngredients);
        else if (baseIngredient is PuddleBaseIngredientType puddleBase)
            OnEffectTriggerExit(other, effect, puddleBase, modifierIngredients);
    }

    public virtual void OnEffectSetup(PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectSetup(PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectSetup(PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectSetup(PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}

    public virtual void OnEffectStart(PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectStart(PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectStart(PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectStart(PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}

    public virtual void OnEffectUpdate(PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectUpdate(PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectUpdate(PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectUpdate(PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}

    public virtual void OnEffectEnd(PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectEnd(PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectEnd(PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectEnd(PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}

    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerEnter(Collider other, PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}

    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, NoBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, CloudBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, CubeBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
    public virtual void OnEffectTriggerExit(Collider other, PotionEffect effect, PuddleBaseIngredientType baseIngredient, List<ModifierIngredientType> modifierIngredients) {}
}