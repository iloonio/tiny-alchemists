
using UnityEngine;

public class CubeBaseEffect : BaseEffect
{
    public float AuraRadius;

    public CubeBaseEffect(float duration, float auraRadius) : base(duration)
    {
        AuraRadius = auraRadius;
    }

    public override void OnEffectSetup(PotionEffect effect)
    {
        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>())
        {
            renderer.material.SetColor("_BaseColor", effect.Color);
        }
    }
}