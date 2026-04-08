using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Spawned by the Fire+Crystal potion. Creates a burning AOE zone that
/// sets fire to anything inside for its duration, then self-destructs.
///
/// UNITY SETUP:
///   - This is instantiated at runtime by Potion.ExplodeEffect.
///   - No manual setup needed; the Potion script creates a primitive sphere,
///     adds this component, and configures it.
public class FireCrystalZone : MonoBehaviour
{
    public float duration = 5f;
    public float radius = 4f;
    public float tickInterval = 0.5f;

    private float _elapsed;

    void Start()
    {
        // Visual: semi-transparent red/orange sphere
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color c = new Color(1f, 0.3f, 0f, 0.25f);
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = c;
            // Make it transparent
            rend.material.SetFloat("_Mode", 3);
            rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            rend.material.SetInt("_ZWrite", 0);
            rend.material.DisableKeyword("_ALPHATEST_ON");
            rend.material.EnableKeyword("_ALPHABLEND_ON");
            rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            rend.material.renderQueue = 3000;
        }

        transform.localScale = Vector3.one * radius * 2f;
        StartCoroutine(BurnZoneRoutine());
    }

    private IEnumerator BurnZoneRoutine()
    {
        while (_elapsed < duration)
        {
            // Overlap sphere to find everything inside
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var col in hits)
            {
                // Burn players
                var sem = col.GetComponent<StatusEffectManager>();
                if (sem != null && !sem.IsOnFire)
                {
                    sem.ApplyFire();
                }

                // Burn flammables
                var flam = col.GetComponent<FlammableObject>();
                if (flam != null)
                {
                    flam.Ignite();
                }
            }

            yield return new WaitForSeconds(tickInterval);
            _elapsed += tickInterval;
        }

        Destroy(gameObject);
    }
}
