
using System.Collections.Generic;
using UnityEngine;

public class CubeBaseEffect : BaseEffect
{
    public float AuraRadius;

    public CubeBaseEffect(float duration, float auraRadius) : base(duration)
    {
        AuraRadius = auraRadius;
    }
}