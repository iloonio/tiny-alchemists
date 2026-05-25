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

    // ── No base ──────────────────────────────────────────────
    // Affected players/objects/miasma set on fire
    public override void OnEffectStart(PotionEffect effect, NoBaseEffect noBaseEffect, List<ModifierEffect> modifierEffects)
    {
        foreach (Collider collider in Physics.OverlapSphere(effect.transform.position, noBaseEffect.Radius))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }        

        MiasmaManager.Instance.DestroySphere(effect.transform.position, noBaseEffect.Radius);
    }

    // ── Cloud base ──────────────────────────────────────────────
    // Affected players/objects/miasma set on fire
    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect cloudBaseEffect, List<ModifierEffect> modifierEffects)
    {
        foreach (Collider collider in cloudBaseEffect.Affected)
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }

        MiasmaManager.Instance.DestroySphere(effect.transform.position, effect.GetComponent<SphereCollider>().radius);
    }

    // ── Cube base ──────────────────────────────────────────────
    // Players/objects/miasma inside aura set on fire
    public override void OnEffectUpdate(PotionEffect effect, CubeBaseEffect cubeBaseEffect, List<ModifierEffect> modifierEffects)
    {   
        Vector3 halfExtents = effect.GetComponent<Collider>().bounds.size / 2 + Vector3.one * cubeBaseEffect.AuraRadius;

        foreach (Collider collider in Physics.OverlapBox(effect.transform.position, halfExtents))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }

        MiasmaManager.Instance.DestroySphere(effect.transform.position, halfExtents.magnitude);
    }

    // ── Puddle base ──────────────────────────────────────────────
    // Players/objects/miasma inside aura set on fire
    public override void OnEffectUpdate(PotionEffect effect, PuddleBaseEffect puddleBaseEffect, List<ModifierEffect> modifierEffects)
    {
        foreach (Collider collider in Physics.OverlapSphere(effect.transform.position, puddleBaseEffect.AuraRadius))
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
        }

        MiasmaManager.Instance.DestroySphere(effect.transform.position, puddleBaseEffect.AuraRadius);
    }
}
