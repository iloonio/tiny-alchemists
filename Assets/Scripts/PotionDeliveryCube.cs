using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  PotionDeliveryCube.cs — Networked physics cube (Object base)
//
//  ARCHITECTURE:
//    - Spawned by server via NetworkedPotion.SpawnNetworkedObject()
//    - Server runs the lifetime coroutine + fire tick
//    - Visual sync happens via NetworkTransform (on the prefab)
//    - Fire effects on players go through StatusEffectManager
//      (server sets NetworkVariable → clients react)
//
//  CURRENT SCOPE (Object + Fire + Size):
//    Fire:  Ignites players on touch + objects within ~1 unit
//    Size:  Larger cube (handled by Configure's size param)
//
//  UNITY SETUP:
//    - Create a Cube prefab with:
//      NetworkObject, NetworkTransform, Rigidbody,
//      BoxCollider, Renderer, PotionDeliveryCube
//    - Register in NetworkManager's prefab list
//    - Assign as cubePrefab on NetworkedPotion
// ═══════════════════════════════════════════════════════════════

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public class PotionDeliveryCube : NetworkBehaviour
{
    [HideInInspector] public float duration = 120f;
    [HideInInspector] public float cubeSize = 1.5f;
    [HideInInspector] public List<IngredientType> modifiers;

    private float _tickInterval = 0.5f;
    private float _fireRadius = 1f;
    private bool _hasFire;
    private Rigidbody _rb;

    /// <summary>
    /// Called by NetworkedPotion.SpawnNetworkedObject on the server,
    /// BEFORE NetworkObject.Spawn(). Configures size, modifiers, physics.
    /// </summary>
    public void Configure(float size, float dur, List<IngredientType> mods)
    {
        cubeSize = size;
        duration = dur;
        modifiers = mods ?? new List<IngredientType>();
        _hasFire = modifiers.Contains(IngredientType.Fire);

        transform.localScale = Vector3.one * cubeSize;

        _rb = GetComponent<Rigidbody>();
        _rb.mass = cubeSize * 5f;

        // Visual tint (server-side, synced via NetworkVariable or RPC if needed)
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = _hasFire
                ? new Color(1f, 0.3f, 0f)  // burning orange
                : new Color(0.6f, 0.8f, 1f); // icy blue
        }
    }

    public override void OnNetworkSpawn()
    {
        // Sync visual on late-joining clients
        ApplyVisualsClientRpc(cubeSize, _hasFire);

        // Only server runs the lifetime + fire tick
        if (IsServer)
        {
            StartCoroutine(CubeLifetimeRoutine());
        }
    }

    [ClientRpc]
    private void ApplyVisualsClientRpc(float size, bool hasFire)
    {
        transform.localScale = Vector3.one * size;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = hasFire
                ? new Color(1f, 0.3f, 0f)
                : new Color(0.6f, 0.8f, 1f);
        }
    }

    // ── Server-only: lifetime + fire tick ──

    private IEnumerator CubeLifetimeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Fire: continuously ignite objects within ~1 unit
            if (_hasFire)
            {
                Collider[] nearby = Physics.OverlapSphere(
                    transform.position, _fireRadius + cubeSize * 0.5f);

                foreach (var col in nearby)
                {
                    if (col.gameObject == gameObject) continue;

                    // Flammable environment objects
                    var flam = col.GetComponent<FlammableObject>();
                    if (flam != null) flam.IgniteServer();

                    // Players touching within radius
                    var sem = col.GetComponent<StatusEffectManager>();
                    if (sem != null && !sem.IsOnFireNet.Value) sem.ApplyFire();
                }
            }

            yield return new WaitForSeconds(_tickInterval);
            elapsed += _tickInterval;
        }

        Debug.Log("<color=blue>[Cube]</color> Potion cube expired.");

        // Despawn across the network (not Destroy!)
        GetComponent<NetworkObject>().Despawn();
    }

    // ── Server-only: contact effects ──

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (_hasFire)
        {
            var sem = collision.gameObject.GetComponent<StatusEffectManager>();
            if (sem != null) sem.ApplyFire();

            var flam = collision.gameObject.GetComponent<FlammableObject>();
            if (flam != null) flam.IgniteServer();
        }
    }
}
