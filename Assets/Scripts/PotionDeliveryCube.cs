using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  PotionDeliveryCube.cs — Physics cube spawned by "Object" base
//
//    Fire:     "ignites players touching cube AND objects within ~1 unit"
//    Float:    "cube unaffected by gravity and has more drag"
//    Bouncy:   "cube is bouncy" (PhysicsMaterial)
//    Magnetic: "cube is magnetic" (registered in MagneticRegistry)
//    Size:     ~4 unit side length (handled at spawn)
//
//  Special Interactions:
//    Size+Magnetic: heavier mass → stronger attraction (automatic)
//    Bouncy+Magnetic: cube repels magnetic objects

public class PotionDeliveryCube : MonoBehaviour
{
    [HideInInspector] public float duration = 120f;
    [HideInInspector] public float cubeSize = 1.5f;
    [HideInInspector] public List<IngredientType> modifiers;

    private float _tickInterval = 0.5f;
    private float _fireRadius = 1f; 
    private bool _hasFire;
    private bool _hasMagnetic;
    private Rigidbody _rb;

    public void Configure(float size, float dur, List<IngredientType> mods)
    {
        cubeSize = size;
        duration = dur;
        modifiers = mods ?? new List<IngredientType>();
        _hasFire = modifiers.Contains(IngredientType.Fire);
        _hasMagnetic = modifiers.Contains(IngredientType.Magnetic);

        transform.localScale = Vector3.one * cubeSize;

        // ── Rigidbody ──
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();

        _rb.mass = cubeSize * 5f;

        // Size+Magnetic special: larger cube has more mass → automatic stronger attraction

        // Float: disable gravity + more drag
        if (modifiers.Contains(IngredientType.Float))
        {
            _rb.useGravity = false;
            _rb.linearDamping = 3f;
            _rb.angularDamping = 2f;
        }

        // Bouncy: bouncy PhysicsMaterial
        if (modifiers.Contains(IngredientType.Bouncy))
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                PhysicsMaterial bounceMat = new PhysicsMaterial("BouncyCube");
                bounceMat.bounciness = 0.9f;
                bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
                col.material = bounceMat;
            }
        }

        // Magnetic: register cube in MagneticRegistry
        if (_hasMagnetic)
        {
            bool repels = modifiers.Contains(IngredientType.Bouncy); // Bouncy+Magnetic special
            MagneticRegistry.Instance.Register(_rb, duration, repels);
        }

        
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            if (_hasFire)
                rend.material.color = new Color(1f, 0.3f, 0f);
            else
                rend.material.color = new Color(0.6f, 0.8f, 1f);
        }

        StartCoroutine(CubeLifetimeRoutine());
    }

    private IEnumerator CubeLifetimeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Fire: continuously ignite objects within ~1 unit
            if (_hasFire)
            {
                Collider[] nearby = Physics.OverlapSphere(transform.position, _fireRadius + cubeSize * 0.5f);
                foreach (var col in nearby)
                {
                    if (col.gameObject == gameObject) continue;

                    var flam = col.GetComponent<FlammableObject>();
                    if (flam != null) flam.Ignite();

                    // Players are handled via OnCollisionEnter (direct touch)
                }
            }

            yield return new WaitForSeconds(_tickInterval);
            elapsed += _tickInterval;
        }

        // Cleanup
        if (_hasMagnetic)
            MagneticRegistry.Instance.Unregister(_rb);

        Debug.Log("<color=blue>[Cube]</color> Potion cube expired.");
        Destroy(gameObject);
    }

    // Contact effects 
    private void OnCollisionEnter(Collision collision)
    {
        // Fire: ignite players on touch
        if (_hasFire)
        {
            var sem = collision.gameObject.GetComponent<StatusEffectManager>();
            if (sem != null) sem.ApplyFire();

            var flam = collision.gameObject.GetComponent<FlammableObject>();
            if (flam != null) flam.Ignite();
        }

        // Bouncy: extra knockback on contact
        if (modifiers.Contains(IngredientType.Bouncy))
        {
            Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
            if (otherRb != null && !otherRb.isKinematic)
            {
                Vector3 away = (collision.transform.position - transform.position).normalized;
                otherRb.AddForce((away + Vector3.up * 0.5f) * 8f, ForceMode.Impulse);
            }
        }
    }

    void OnDestroy()
    {
        // Safety: unregister if destroyed early
        if (_hasMagnetic && _rb != null)
            MagneticRegistry.Instance.Unregister(_rb);
    }
}
