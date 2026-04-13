using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//  PotionDeliveryZone.cs — Persistent AoE zone for Cloud & Puddle
//
//  Cloud = sphere trigger zone at impact point
//  Puddle = flat disc on surface
//
//  Now passes DeliveryContext to PotionModifierHandler so
//  each modifier behaves differently per delivery shape.
//
//  Puddle + Bouncy: the zone itself gets a bouncy PhysicsMaterial
//  so things physically bounce off it on collision.

public enum DeliveryShape { Cloud, Puddle }

public class PotionDeliveryZone : MonoBehaviour
{
    [HideInInspector] public DeliveryShape shape;
    [HideInInspector] public float radius = 3f;
    [HideInInspector] public float duration = 120f;
    [HideInInspector] public List<IngredientType> modifiers;

    private float _tickInterval = 0.5f;
    private bool _hasFireSparkle;

    public void Configure(DeliveryShape deliveryShape, float zoneRadius, float zoneDuration,
                          List<IngredientType> mods, Vector3 surfaceNormal)
    {
        shape = deliveryShape;
        radius = zoneRadius;
        duration = zoneDuration;
        modifiers = mods ?? new List<IngredientType>();

        _hasFireSparkle = modifiers.Contains(IngredientType.Fire) && modifiers.Contains(IngredientType.Sparkle);

        // ── Shape the visual ──
        if (shape == DeliveryShape.Cloud)
        {
            transform.localScale = Vector3.one * radius * 2f;
        }
        else // Puddle
        {
            transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);
            if (surfaceNormal != Vector3.zero)
                transform.rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        }

        // ── Visual: semi-transparent ──
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color c;
            if (shape == DeliveryShape.Cloud)
                c = new Color(0.8f, 0.8f, 1f, 0.2f);
            else
                c = new Color(0.3f, 0.1f, 0.8f, 0.3f);

            if (modifiers.Contains(IngredientType.Fire))
                c = new Color(1f, 0.4f, 0.1f, 0.25f);
            if (_hasFireSparkle)
                c = new Color(1f, 0.8f, 0.1f, 0.35f); // bright for explosions

            SetTransparentMaterial(rend, c);
        }

        // ── Puddle + Bouncy: add bouncy collider to the zone itself ──
        if (shape == DeliveryShape.Puddle && modifiers.Contains(IngredientType.Bouncy))
        {
            // Re-add a solid collider (we destroyed the default one in Potion.cs)
            BoxCollider solidCol = gameObject.AddComponent<BoxCollider>();
            solidCol.isTrigger = false;
            solidCol.size = Vector3.one;

            PhysicsMaterial bounceMat = new PhysicsMaterial("BouncyPuddle");
            bounceMat.bounciness = 0.95f;
            bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
            solidCol.material = bounceMat;

            // Need a kinematic Rigidbody so collider works
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // ── Magnetic: register the zone itself if Object-like (cube handled elsewhere) ──
        // For zones, magnetic is applied per-tick to targets inside

        StartCoroutine(ZoneTickRoutine());
    }

    private IEnumerator ZoneTickRoutine()
    {
        float elapsed = 0f;

        // For Fire+Sparkle special: track explosion timing
        float explosionTimer = 0f;
        float explosionInterval = 2f; // explode every 2 seconds

        DeliveryContext ctx = (shape == DeliveryShape.Cloud)
            ? DeliveryContext.Cloud
            : DeliveryContext.Puddle;

        while (elapsed < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            // Filter out self
            var filtered = new List<Collider>();
            foreach (var h in hits)
            {
                if (h.gameObject != gameObject)
                    filtered.Add(h);
            }

            // ── Fire+Sparkle special: repeated explosions ──
            if (_hasFireSparkle)
            {
                explosionTimer += _tickInterval;
                if (explosionTimer >= explosionInterval)
                {
                    explosionTimer = 0f;
                    // Apply explosion to current targets
                    PotionModifierHandler.ApplyModifiers(
                        filtered.ToArray(),
                        modifiers,
                        transform.position,
                        ctx
                    );
                }
                // Between explosions, only apply non-Fire-Sparkle modifiers
                else
                {
                    var nonSpecial = new List<IngredientType>();
                    foreach (var m in modifiers)
                    {
                        if (m != IngredientType.Fire && m != IngredientType.Sparkle)
                            nonSpecial.Add(m);
                    }
                    if (nonSpecial.Count > 0)
                    {
                        PotionModifierHandler.ApplyModifiers(
                            filtered.ToArray(),
                            nonSpecial,
                            transform.position,
                            ctx
                        );
                    }
                }
            }
            else
            {
                // Normal tick: apply all modifiers
                PotionModifierHandler.ApplyModifiers(
                    filtered.ToArray(),
                    modifiers,
                    transform.position,
                    ctx
                );
            }

            yield return new WaitForSeconds(_tickInterval);
            elapsed += _tickInterval;
        }

        Debug.Log($"<color=magenta>[Zone]</color> {shape} expired after {duration}s");
        Destroy(gameObject);
    }

    private void SetTransparentMaterial(Renderer rend, Color c)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = c;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        rend.material = mat;
    }
}
