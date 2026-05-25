
using System.Collections.Generic;
using UnityEngine;

public class PuddleBaseEffect : BaseEffect
{

    public float AuraRadius;

    public PuddleBaseEffect(float duration, float auraRadius) : base(duration)
    {
        AuraRadius = auraRadius;
    }

    public override void OnEffectSetup(PotionEffect effect)
    {
        effect.GetComponentInChildren<Renderer>().material.SetColor("_Color", effect.Color);
    }
}