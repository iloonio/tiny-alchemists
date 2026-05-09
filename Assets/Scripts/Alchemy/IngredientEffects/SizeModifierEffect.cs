
using System.Collections.Generic;
using UnityEngine;

public class SizeModifierEffect : ModifierEffect
{
    public float ScaleMultiplier;
    public SizeModifierEffect(float scaleMultiplier)
    {
        ScaleMultiplier = scaleMultiplier;
    }

    // NO BASE
    public override void OnEffectSetup(PotionEffect effect, NoBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        baseEffect.Radius *= ScaleMultiplier;
        effect.transform.localScale *= ScaleMultiplier;
    }

    // CLOUD BASE
    public override void OnEffectSetup(PotionEffect effect, CloudBaseEffect baseEffect, List<ModifierEffect> modifierEffects)
    {
        effect.transform.localScale *= ScaleMultiplier;
    }

}