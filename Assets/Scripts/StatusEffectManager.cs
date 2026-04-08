using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to any GameObject that can receive status effects (Players, environment objects).
/// For Players: also requires PlayerMovementFPS on the same object.
/// 
/// UNITY SETUP:
///   - Tag players as "Player"
///   - Tag burnable props as "Flammable" (and add FlammableObject.cs to them)
///   - Attach this component to every Player prefab
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // ── Public state flags (read by PlayerMovementFPS / PlayerInteraction) ──
    [HideInInspector] public bool IsOnFire;
    [HideInInspector] public bool IsCrystallized;

    // ── Tuning ──
    [Header("On-Fire Settings")]
    [Tooltip("How long the fire lasts on a player (seconds)")]
    public float fireDuration = 5f;
    [Tooltip("Intensity of mouse/camera jitter while on fire")]
    public float jitterIntensity = 3f;
    [Tooltip("Strength of sporadic forward pushes while on fire")]
    public float sporadicPushStrength = 1.5f;
    [Tooltip("Average interval between sporadic pushes (seconds)")]
    public float pushInterval = 0.4f;

    [Header("Crystal Settings")]
    [Tooltip("How long the crystal encasement lasts (seconds)")]
    public float crystalDuration = 4f;
    [Tooltip("Extra downward force applied when crystallized")]
    public float crystalGravityMultiplier = 5f;

    // ── Internal ──
    private Coroutine _fireRoutine;
    private Coroutine _crystalRoutine;
    private Rigidbody _rb;

    // Jitter value consumed each frame by PlayerMovementFPS
    [HideInInspector] public Vector2 cameraJitter;
    // Sporadic forward impulse consumed each FixedUpdate by PlayerMovementFPS
    [HideInInspector] public float sporadicForward;

    // Optional: visual placeholder for crystal shell (assign in Inspector or spawned at runtime)
    [Header("Visuals (Optional)")]
    public GameObject crystalShellPrefab;
    private GameObject _activeCrystalShell;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // ──────────────────────────────────────────────
    //  PUBLIC API – called by Potion.ExplodeEffect
    // ──────────────────────────────────────────────

    /// <summary>Apply the On-Fire effect. Safe to call on players or environment objects.</summary>
    public void ApplyFire(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : fireDuration;

        // Environment path: if this is a FlammableObject, let it handle itself
        var flammable = GetComponent<FlammableObject>();
        if (flammable != null)
        {
            flammable.Ignite();
            return;
        }

        // Player path
        if (_fireRoutine != null) StopCoroutine(_fireRoutine);
        _fireRoutine = StartCoroutine(FireRoutine(dur));
    }

    /// <summary>Apply the Crystallized effect. Only meaningful on players.</summary>
    public void ApplyCrystal(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : crystalDuration;
        if (_crystalRoutine != null) StopCoroutine(_crystalRoutine);
        _crystalRoutine = StartCoroutine(CrystalRoutine(dur));
    }

    // ──────────────────────────────────────────────
    //  COROUTINES
    // ──────────────────────────────────────────────

    private IEnumerator FireRoutine(float duration)
    {
        IsOnFire = true;
        Debug.Log($"<color=orange>[Status]</color> {gameObject.name} is ON FIRE for {duration}s");

        float elapsed = 0f;
        float nextPush = UnityEngine.Random.Range(0.1f, pushInterval);

        while (elapsed < duration)
        {
            // Camera jitter – random offset consumed in PlayerMovementFPS.HandleLook
            cameraJitter = new Vector2(
                UnityEngine.Random.Range(-jitterIntensity, jitterIntensity),
                UnityEngine.Random.Range(-jitterIntensity, jitterIntensity)
            );

            // Sporadic forward movement
            nextPush -= Time.deltaTime;
            if (nextPush <= 0f)
            {
                sporadicForward = sporadicPushStrength;
                nextPush = UnityEngine.Random.Range(0.2f, pushInterval * 2f);
            }
            else
            {
                sporadicForward = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Clean up
        cameraJitter = Vector2.zero;
        sporadicForward = 0f;
        IsOnFire = false;
        _fireRoutine = null;
        Debug.Log($"<color=orange>[Status]</color> {gameObject.name} fire extinguished.");
    }

    private IEnumerator CrystalRoutine(float duration)
    {
        IsCrystallized = true;
        Debug.Log($"<color=cyan>[Status]</color> {gameObject.name} is CRYSTALLIZED for {duration}s");

        // Spawn visual shell
        if (crystalShellPrefab != null)
        {
            _activeCrystalShell = Instantiate(crystalShellPrefab, transform.position, Quaternion.identity, transform);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Force downward – plummet to ground
            if (_rb != null)
            {
                _rb.AddForce(Vector3.down * crystalGravityMultiplier, ForceMode.Acceleration);
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Clean up
        if (_activeCrystalShell != null) Destroy(_activeCrystalShell);
        IsCrystallized = false;
        _crystalRoutine = null;
        Debug.Log($"<color=cyan>[Status]</color> {gameObject.name} crystal shattered.");
    }
}
