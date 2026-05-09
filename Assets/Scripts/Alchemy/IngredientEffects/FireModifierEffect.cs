

using System.Collections.Generic;
using UnityEngine;

public class FireModifierEffect : ModifierEffect
{
    public Status FireStatus;
    public float FireStatusDuration;

    public FireModifierEffect(Status fireStatus, float fireStatusDuration)
    {
        FireStatus = fireStatus;
        FireStatusDuration = fireStatusDuration;
    }

    // NO BASE
    public override void OnEffectStart(PotionEffect effect, NoBaseEffect noBaseEffect, List<ModifierEffect> modifierEffects)
    {
        foreach (var collider in Physics.OverlapSphere(effect.transform.position, noBaseEffect.Radius))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
            {
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
            }
        }        
    }

    // CLOUD BASE
    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        foreach (Collider collider in baseEffect.Affected)
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
            {
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
            }
        }
    }

}