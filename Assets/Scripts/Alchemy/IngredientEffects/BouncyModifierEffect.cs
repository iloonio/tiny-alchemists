using System.Collections.Generic;
using UnityEngine;

public class BouncyModifierEffect : ModifierEffect
{
    public Status BouncyStatus;
    public float StatusDuration;
    public float KnockbackMultiplier;
    public float Bounciness;

    public BouncyModifierEffect(Status bouncyStatus, float statusDuration, float knockbackMultiplier, float bounciness)
    {
        BouncyStatus = bouncyStatus;
        StatusDuration = statusDuration;
        KnockbackMultiplier = knockbackMultiplier;
        Bounciness = bounciness;
    }

    // ── No base ──────────────────────────────────────────────
    // Increased knockback effect

    public override void OnEffectSetup(PotionEffect effect, NoBaseEffect noBase, List<ModifierEffect> modifierEffects)
    {
        noBase.ExplosionForce *= KnockbackMultiplier;
    }

    // ── Cloud base ───────────────────────────────────────────
    // Affected players are bouncy

    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect cloudBase, List<ModifierEffect> modifierEffects)
    {
        foreach (Collider collider in cloudBase.Affected)
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(BouncyStatus, 0.1f);
        }
    }

    // ── Cube base ────────────────────────────────────────────
    // Cube is bouncy

    public override void OnEffectSetup(PotionEffect effect, CubeBaseEffect cubeBase, List<ModifierEffect> modifierEffects)
    {
        if (effect.TryGetComponent(out Collider col))
        {
            PhysicsMaterial mat = new PhysicsMaterial("BouncyCube")
            {
                bounciness = Bounciness,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            col.material = mat;
        }
    }

    // ── Puddle base ──────────────────────────────────────────
    // Surface is bouncy

    public override void OnEffectSetup(PotionEffect effect, PuddleBaseEffect puddleBase, List<ModifierEffect> modifierEffects)
    {
        if (effect.TryGetComponent(out Collider col))
        {
            PhysicsMaterial mat = new PhysicsMaterial("BouncyPuddle")
            {
                bounciness = Bounciness,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            col.material = mat;
        }
    }
}