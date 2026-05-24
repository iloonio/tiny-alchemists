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

        MiasmaManager.Instance.DestroySphere(effect.transform.position, noBaseEffect.Radius);
    }

    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect cloudBaseEffect, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (Collider collider in cloudBaseEffect.Affected)
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }

        MiasmaManager.Instance.DestroySphere(effect.transform.position, effect.GetComponent<SphereCollider>().radius);
    }
}
