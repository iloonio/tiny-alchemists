using System.Collections;
using UnityEngine;

//  StatusEffectManager.cs — All status effects on a player
//
//  FIRE on Player:
//    - Move faster (speed multiplier)
//    - Occasionally moved in random HORIZONTAL directions
//    - Immune to miasma for ~5s
//    - Lasts ~5s
//
//  FLOAT on Player:
//    - Gravity disabled for duration
//
//  BOUNCY on Player:
//    - PhysicMaterial with high bounciness for duration

public class StatusEffectManager : MonoBehaviour
{
    // ── Public state flags ──
    [HideInInspector] public bool IsOnFire;
    [HideInInspector] public bool IsCrystallized;
    [HideInInspector] public bool IsFloating;
    [HideInInspector] public bool IsBouncy;
    [HideInInspector] public bool IsMiasmaImmune;

    // ── Fire: speed multiplier read by PlayerMovement ──
    [HideInInspector] public float fireSpeedMultiplier = 1f;
    // ── Fire: random horizontal push read by PlayerMovement ──
    [HideInInspector] public Vector3 fireRandomPush;

    [Header("On-Fire Settings")]
    public float fireDuration = 5f;
    [Tooltip("Speed multiplier while on fire (GDD: move faster)")]
    public float fireSpeedBoost = 1.6f;
    [Tooltip("Strength of random horizontal pushes")]
    public float fireRandomPushStrength = 2f;
    [Tooltip("Average interval between random pushes (seconds)")]
    public float pushInterval = 0.4f;

    [Header("Float Settings")]
    public float floatDuration = 5f;

    [Header("Bouncy Settings")]
    public float bouncyDuration = 5f;
    public float bounciness = 0.9f;

    [Header("Crystal Settings")]
    public float crystalDuration = 4f;
    public float crystalGravityMultiplier = 5f;

    [Header("Visuals (Optional)")]
    public GameObject crystalShellPrefab;

    private Coroutine _fireRoutine;
    private Coroutine _crystalRoutine;
    private Coroutine _floatRoutine;
    private Coroutine _bouncyRoutine;
    private Rigidbody _rb;
    private Collider _col;
    private PhysicsMaterial _originalPhysMat;
    private GameObject _activeCrystalShell;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        if (_col != null) _originalPhysMat = _col.material;
    }

    //  PUBLIC API
    public void ApplyFire(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : fireDuration;

        // Environment path
        var flammable = GetComponent<FlammableObject>();
        if (flammable != null)
        {
            flammable.Ignite();
            return;
        }

        // Player path — restart timer
        if (_fireRoutine != null) StopCoroutine(_fireRoutine);
        _fireRoutine = StartCoroutine(FireRoutine(dur));
    }

    public void ApplyCrystal(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : crystalDuration;
        if (_crystalRoutine != null) StopCoroutine(_crystalRoutine);
        _crystalRoutine = StartCoroutine(CrystalRoutine(dur));
    }


    // Disable gravity for duration. No-base+Float = 5s one-shot.
    // Cloud+Float calls with -1 (permanent while in zone, re-applied each tick).
    public void ApplyFloat(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : floatDuration;

        // If already floating, just extend
        if (IsFloating) return;

        if (_floatRoutine != null) StopCoroutine(_floatRoutine);
        _floatRoutine = StartCoroutine(FloatRoutine(dur));
    }

    // Make the player bouncy for a duration.
    public void ApplyBouncy(float overrideDuration = -1f)
    {
        float dur = overrideDuration > 0 ? overrideDuration : bouncyDuration;
        if (IsBouncy) return;

        if (_bouncyRoutine != null) StopCoroutine(_bouncyRoutine);
        _bouncyRoutine = StartCoroutine(BouncyRoutine(dur));
    }
    
    //  COROUTINES

    // ── FIRE ──
    private IEnumerator FireRoutine(float duration)
    {
        IsOnFire = true;
        IsMiasmaImmune = true;
        fireSpeedMultiplier = fireSpeedBoost;

        Debug.Log($"<color=orange>[Status]</color> {gameObject.name} ON FIRE for {duration}s (speed x{fireSpeedBoost}, miasma immune)");

        float elapsed = 0f;
        float nextPush = Random.Range(0.1f, pushInterval);

        while (elapsed < duration)
        {
            // Random HORIZONTAL push (GDD: "occasionally moved in random horizontal directions")
            nextPush -= Time.deltaTime;
            if (nextPush <= 0f)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                fireRandomPush = new Vector3(
                    Mathf.Cos(angle) * fireRandomPushStrength,
                    0f,
                    Mathf.Sin(angle) * fireRandomPushStrength
                );
                nextPush = Random.Range(0.2f, pushInterval * 2f);
            }
            else
            {
                fireRandomPush = Vector3.zero;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Clean up
        fireRandomPush = Vector3.zero;
        fireSpeedMultiplier = 1f;
        IsOnFire = false;
        IsMiasmaImmune = false;
        _fireRoutine = null;
        Debug.Log($"<color=orange>[Status]</color> {gameObject.name} fire extinguished.");
    }

    // ── CRYSTAL ──
    private IEnumerator CrystalRoutine(float duration)
    {
        IsCrystallized = true;
        Debug.Log($"<color=cyan>[Status]</color> {gameObject.name} CRYSTALLIZED for {duration}s");

        if (crystalShellPrefab != null)
            _activeCrystalShell = Instantiate(crystalShellPrefab, transform.position, Quaternion.identity, transform);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (_rb != null)
                _rb.AddForce(Vector3.down * crystalGravityMultiplier, ForceMode.Acceleration);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (_activeCrystalShell != null) Destroy(_activeCrystalShell);
        IsCrystallized = false;
        _crystalRoutine = null;
        Debug.Log($"<color=cyan>[Status]</color> {gameObject.name} crystal shattered.");
    }

    // ── FLOAT ──
    private IEnumerator FloatRoutine(float duration)
    {
        IsFloating = true;
        bool wasGravity = _rb != null && _rb.useGravity;

        if (_rb != null) _rb.useGravity = false;
        Debug.Log($"<color=white>[Status]</color> {gameObject.name} FLOATING for {duration}s");

        yield return new WaitForSeconds(duration);

        if (_rb != null) _rb.useGravity = wasGravity;
        IsFloating = false;
        _floatRoutine = null;
        Debug.Log($"<color=white>[Status]</color> {gameObject.name} float ended.");
    }

    // ── BOUNCY ──
    private IEnumerator BouncyRoutine(float duration)
    {
        IsBouncy = true;
        Debug.Log($"<color=green>[Status]</color> {gameObject.name} BOUNCY for {duration}s");

        // Apply bouncy physics material to player collider
        if (_col != null)
        {
            PhysicsMaterial bounceMat = new PhysicsMaterial("BouncePlayer");
            bounceMat.bounciness = bounciness;
            bounceMat.bounceCombine = PhysicsMaterialCombine.Maximum;
            _col.material = bounceMat;
        }

        yield return new WaitForSeconds(duration);

        // Restore original
        if (_col != null) _col.material = _originalPhysMat;
        IsBouncy = false;
        _bouncyRoutine = null;
        Debug.Log($"<color=green>[Status]</color> {gameObject.name} bounce ended.");
    }
}
