using System.Collections.Generic;
using UnityEngine;

public class FloatModifierEffect : ModifierEffect
{
    public Status FloatStatus;
    public float StatusDuration;
    public float CubeDrag;
    public float PuddleUpwardForce;

    private HashSet<Collider> _puddleAffected = new();

    public FloatModifierEffect(Status floatStatus, float statusDuration, float cubeDrag, float puddleUpwardForce)
    {
        FloatStatus = floatStatus;
        StatusDuration = statusDuration;
        CubeDrag = cubeDrag;
        PuddleUpwardForce = puddleUpwardForce;
    }

    // ── No base ──────────────────────────────────────────────
    // Affected players and objects are unaffected by gravity for ~5s

    public override void OnEffectStart(PotionEffect effect, NoBaseEffect noBase, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (var collider in Physics.OverlapSphere(effect.transform.position, noBase.Radius))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FloatStatus, StatusDuration);
        }
    }

    // ── Cloud base ───────────────────────────────────────────
    // Affected players are continuously unaffected by gravity

    public override void OnEffectUpdate(PotionEffect effect, CloudBaseEffect cloudBase, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (Collider collider in cloudBase.Affected)
        {
            if (collider != null && collider.TryGetComponent(out StatusAffectable statusAffectable))
                statusAffectable.AddStatus(FloatStatus, StatusDuration);
        }
    }

    // ── Cube base ────────────────────────────────────────────
    // Cube is unaffected by gravity and has more drag

    public override void OnEffectSetup(PotionEffect effect, CubeBaseEffect cubeBase, List<ModifierEffect> modifierEffects)
    {
        if (effect.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.linearDamping = CubeDrag;
        }
    }

    // ── Puddle base ──────────────────────────────────────────
    // Continuous upward force pushing away from puddle

    public override void OnEffectTriggerEnter(Collider other, PotionEffect effect, PuddleBaseEffect puddleBase, List<ModifierEffect> modifierEffects)
    {
        _puddleAffected.Add(other);
    }

    public override void OnEffectTriggerExit(Collider other, PotionEffect effect, PuddleBaseEffect puddleBase, List<ModifierEffect> modifierEffects)
    {
        _puddleAffected.Remove(other);
    }

    public override void OnEffectUpdate(PotionEffect effect, PuddleBaseEffect puddleBase, List<ModifierEffect> modifierEffects)
    {
        if (!effect.IsServer) return;

        foreach (Collider collider in _puddleAffected)
        {
            if (collider == null) continue;

            // Players: apply status (gravity disabled via FloatStatus)
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
            {
                statusAffectable.AddStatus(FloatStatus, StatusDuration);
            }
            // Non-player rigidbodies: apply upward force directly on server
            else if (collider.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(Vector3.up * PuddleUpwardForce, ForceMode.Force);
            }
        }
    }
}