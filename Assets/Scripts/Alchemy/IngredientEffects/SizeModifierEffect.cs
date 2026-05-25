
using System.Collections.Generic;
using UnityEngine;

public class SizeModifierEffect : ModifierEffect
{
    public float ScaleMultiplier;
    public SizeModifierEffect(float scaleMultiplier)
    {
        ScaleMultiplier = scaleMultiplier;
    }

    // ── No base ──────────────────────────────────────────────
    public override void OnEffectSetup(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        baseEffect.Radius *= ScaleMultiplier;
        effect.transform.localScale *= ScaleMultiplier;
    }

    // ── Cloud base ──────────────────────────────────────────────
    public override void OnEffectSetup(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        effect.transform.localScale *= ScaleMultiplier;
    }

    // ── Cube base ──────────────────────────────────────────────
    public override void OnEffectSetup(PotionEffect effect, CubeBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        effect.transform.localScale *= ScaleMultiplier;
    }

    // ── Puddle base ──────────────────────────────────────────────
    public override void OnEffectSetup(PotionEffect effect, PuddleBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        baseEffect.AuraRadius *= ScaleMultiplier;
        effect.transform.localScale = new Vector3(effect.transform.localScale.x * ScaleMultiplier, effect.transform.localScale.y, effect.transform.localScale.z * ScaleMultiplier);
    }
}