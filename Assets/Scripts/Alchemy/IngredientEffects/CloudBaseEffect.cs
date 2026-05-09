
using System.Collections.Generic;
using UnityEngine;

public class CloudBaseEffect : BaseEffect
{
    public HashSet<Collider> Affected = new();
    public CloudBaseEffect(float duration) : base(duration)
    {
        
    }

    public override void OnEffectTriggerEnter(Collider other, PotionEffect effect)
    {
        Affected.Add(other);
    }

    public override void OnEffectTriggerExit(Collider other, PotionEffect effect)
    {
        Affected.Remove(other);
    }

}