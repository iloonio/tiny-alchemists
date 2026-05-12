using System.Collections;
using UnityEngine;
using Unity.Netcode;

// ═══════════════════════════════════════════════════════════════
//  StatusEffectManager.cs — Networked Status Effects
//
//  ARCHITECTURE:
//    Server:  Decides who gets an effect → sets NetworkVariable
//    Client:  Reads NetworkVariable → runs LOCAL coroutine for
//             movement effects (speed boost, push) and visuals
//
//  This means each client controls their own movement response
//  to fire, while the server controls the world state (duration,
//  who is affected, fire spreading).
//
//  CURRENT EFFECTS (Object + Fire + Size scope):
//    - Fire: speed boost + random horizontal push + miasma immunity
//
//  UNITY SETUP:
//    - Attach to Player prefab (needs NetworkObject)
//    - Also attach to any entity that can receive status effects
// ═══════════════════════════════════════════════════════════════

public class StatusEffectManager : NetworkBehaviour
{
    // ══════════════════════════════════════════
    //  NETWORKED STATE (Server writes, everyone reads)
    // ══════════════════════════════════════════

    /// <summary>Synced fire state. Server sets true/false, clients react.</summary>
    public NetworkVariable<bool> IsOnFireNet = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>Fire duration synced so clients know how long to run local effects.</summary>
    public NetworkVariable<float> FireDurationNet = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ══════════════════════════════════════════
    //  LOCAL STATE (read by PlayerMovementFPS on this client)
    // ══════════════════════════════════════════

    [HideInInspector] public bool IsOnFire;
    [HideInInspector] public bool IsCrystallized;
    [HideInInspector] public bool IsFloating;
    [HideInInspector] public bool IsBouncy;
    [HideInInspector] public bool IsMiasmaImmune;

    /// <summary>Speed multiplier while on fire. Read by PlayerMovementFPS.</summary>
    [HideInInspector] public float fireSpeedMultiplier = 1f;

    /// <summary>Random horizontal push. Read by PlayerMovementFPS.</summary>
    [HideInInspector] public Vector3 fireRandomPush;

    // ══════════════════════════════════════════
    //  TUNING
    // ══════════════════════════════════════════

    [Header("On-Fire Settings")]
    public float fireDuration = 5f;
    public float fireSpeedBoost = 1.6f;
    public float fireRandomPushStrength = 0.5f;
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

    // ── Internal ──
    private Coroutine _serverFireRoutine;
    private Coroutine _localFireRoutine;
    private Rigidbody _rb;
    private Collider _col;
    private PhysicsMaterial _originalPhysMat;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        if (_col != null) _originalPhysMat = _col.material;
    }

    // ══════════════════════════════════════════
    //  NETWORK LIFECYCLE
    // ══════════════════════════════════════════

    public override void OnNetworkSpawn()
    {
        // Everyone subscribes to fire state changes
        IsOnFireNet.OnValueChanged += OnFireStateChanged;

        // If we joined mid-fire, handle current state
        if (IsOnFireNet.Value)
            StartLocalFireEffects(FireDurationNet.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsOnFireNet.OnValueChanged -= OnFireStateChanged;
    }

    /// <summary>
    /// Called on ALL instances (server + every client) when fire state changes.
    /// </summary>
    private void OnFireStateChanged(bool wasOnFire, bool isNowOnFire)
    {
        if (isNowOnFire)
        {
            StartLocalFireEffects(FireDurationNet.Value);
        }
        else
        {
            StopLocalFireEffects();
        }
    }

    // ══════════════════════════════════════════
    //  PUBLIC API (called by PotionModifierHandler / Cauldron / etc.)
    // ══════════════════════════════════════════

    /// <summary>
    /// Apply fire effect. Only executes on server.
    /// Server sets NetworkVariables → clients react via OnValueChanged.
    /// </summary>
    public void ApplyFire(float overrideDuration = -1f)
    {
        if (!IsServer) return;

        float dur = overrideDuration > 0 ? overrideDuration : fireDuration;

        // Environment path: delegate to FlammableObject
        var flammable = GetComponent<FlammableObject>();
        if (flammable != null)
        {
            flammable.IgniteServer();
            return;
        }

        // Player path: set networked state (this triggers OnFireStateChanged on all clients)
        FireDurationNet.Value = dur;

        // Server-side timer to auto-clear the fire
        if (_serverFireRoutine != null) StopCoroutine(_serverFireRoutine);
        _serverFireRoutine = StartCoroutine(ServerFireTimerRoutine(dur));
    }

    // ══════════════════════════════════════════
    //  SERVER: Fire duration timer
    //  (only runs on server — manages the NetworkVariable lifecycle)
    // ══════════════════════════════════════════

    private IEnumerator ServerFireTimerRoutine(float duration)
    {
        IsOnFireNet.Value = true;

        Debug.Log($"<color=orange>[Status Server]</color> {gameObject.name} ON FIRE for {duration}s");

        yield return new WaitForSeconds(duration);

        IsOnFireNet.Value = false;
        _serverFireRoutine = null;

        Debug.Log($"<color=orange>[Status Server]</color> {gameObject.name} fire expired.");
    }

    // ══════════════════════════════════════════
    //  LOCAL: Fire gameplay effects
    //  (runs on EVERY instance — each client handles their own
    //   speed/push; server instance also runs for physics authority)
    // ══════════════════════════════════════════

    private void StartLocalFireEffects(float duration)
    {
        if (_localFireRoutine != null) StopCoroutine(_localFireRoutine);
        _localFireRoutine = StartCoroutine(LocalFireEffectsRoutine(duration));
    }

    private void StopLocalFireEffects()
    {
        if (_localFireRoutine != null)
        {
            StopCoroutine(_localFireRoutine);
            _localFireRoutine = null;
        }

        // Reset all local fire state
        fireRandomPush = Vector3.zero;
        fireSpeedMultiplier = 1f;
        IsOnFire = false;
        IsMiasmaImmune = false;

        // TODO: Stop fire VFX here
        Debug.Log($"<color=orange>[Status Local]</color> {gameObject.name} fire visuals/effects off.");
    }

    private IEnumerator LocalFireEffectsRoutine(float duration)
    {
        IsOnFire = true;
        IsMiasmaImmune = true;
        fireSpeedMultiplier = fireSpeedBoost;

        // TODO: Start fire VFX here (particle system, screen overlay, etc.)
        Debug.Log($"<color=orange>[Status Local]</color> {gameObject.name} fire effects ON (speed x{fireSpeedBoost})");

        float elapsed = 0f;
        float nextPush = Random.Range(0.1f, pushInterval);

        while (elapsed < duration)
        {
            // Random horizontal push
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

        // Don't clear state here — wait for OnFireStateChanged(false)
        // The server controls when fire ends, not the local timer
        _localFireRoutine = null;
    }

    // ══════════════════════════════════════════
    //  PLACEHOLDER STUBS (for future effects)
    // ══════════════════════════════════════════

    public void ApplyCrystal(float overrideDuration = -1f)
    {
        // TODO: Implement with NetworkVariable pattern like fire
    }

    public void ApplyFloat(float overrideDuration = -1f)
    {
        // TODO: Implement with NetworkVariable pattern like fire
    }

    public void ApplyBouncy(float overrideDuration = -1f)
    {
        // TODO: Implement with NetworkVariable pattern like fire
    }
}
