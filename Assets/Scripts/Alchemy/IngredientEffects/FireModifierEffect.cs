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

    public override void OnEffectStart(PotionEffect effect, NoBaseEffect noBaseEffect, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (var collider in Physics.OverlapSphere(effect.transform.position, noBaseEffect.Radius))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }        
    }

    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (Collider collider in baseEffect.Affected)
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }
    }
}
