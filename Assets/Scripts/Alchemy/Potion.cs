using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// Potion
/// =============================================================
/// Represents a networked Potion created from a cauldron recipe
/// - Stores base and modifier ingredient IDs and synchronizes them across the network.
/// - Computes and applies a visual color based on ingredient colors.
/// - On high-velocity collisions spawns a PotionEffect and despawns itself.
/// - Server is authoritative for network variables, spawns/despawns, and physics-driven effects.
/// =============================================================
[RequireComponent(typeof(Rigidbody))]
public class Potion : NetworkBehaviour
{
    [SerializeField] private float _breakSpeed = 10f;

    private int _baseIngredientId;
    private List<int> _modifierIngredientIds = new();
    private Color _color;
    private Renderer _renderer;
    private Rigidbody _rb;
    public Rigidbody Rb => _rb;

    private NetworkVariable<int> _baseIngredientIdNetwork = new();
    private NetworkList<int> _modifierIngredientIdsNetwork = new();

    /// Initialize
    /// =============================================================
    /// Stores the provided recipe IDs locally so they can be synchronized on spawn.
    /// - Called by the cauldron before the Potion's NetworkObject is spawned on the server.
    /// - Populates the local _baseIngredientId and _modifierIngredientIds lists; these are
    ///   later copied into networked collections during OnNetworkSpawn when running on the server.
    /// =============================================================
    public void Initialize(int baseIngredientId, List<int> modifierIngredientIds)
    {
        _baseIngredientId = baseIngredientId;
        foreach (var modifierIngredientId in modifierIngredientIds)
        {
            _modifierIngredientIds.Add(modifierIngredientId);
        }
    }

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _rb = GetComponent<Rigidbody>();
    }

    /// OnNetworkSpawn
    /// =============================================================
    /// Synchronizes local and networked recipe state when the NetworkObject spawns.
    /// - On the server: writes local _baseIngredientId and _modifierIngredientIds into
    ///   _baseIngredientIdNetwork and _modifierIngredientIdsNetwork so clients receive the data.
    /// - On clients: reads the networked values back into local fields so client-side visuals
    ///   and effects can be initialized.
    /// - Calls SetColor() after synchronization to apply the correct visual color.
    /// =============================================================
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _baseIngredientIdNetwork.Value = _baseIngredientId;
            foreach (var modifierIngredientId in _modifierIngredientIds)
            {
                _modifierIngredientIdsNetwork.Add(modifierIngredientId);
            }
        }
        else
        {
            _baseIngredientId = _baseIngredientIdNetwork.Value;
            foreach (var modifierIngredientId in _modifierIngredientIdsNetwork)
            {
                _modifierIngredientIds.Add(modifierIngredientId);
            }
        }

        SetColor();
    }

    private void SetColor()
    {
        Color sum = ((IngredientType)_baseIngredientId).Color;

        foreach (var modifierIngredientId in _modifierIngredientIds)
        {
            sum += ((IngredientType)modifierIngredientId).Color;
        }

        _color = sum / (1f + _modifierIngredientIds.Count);
        _renderer.materials[2].color = _color;
    }

    /// OnCollisionEnter
    /// =============================================================
    /// Handles high-impact collisions on the server.
    /// - If the collision relative velocity exceeds _breakSpeed, a PotionEffect prefab
    ///   (from the base ingredient) is instantiated at the contact point, initialized,
    ///   network-spawned, and the Potion NetworkObject is despawned.
    /// - All authoritative spawn/despawn calls and physics checks are gated to the server.
    /// =============================================================
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.relativeVelocity.magnitude < _breakSpeed) return;

        GetComponentInChildren<AudioPlayer>().Play("PotionBreak");

        ContactPoint contact = collision.GetContact(0);
        Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, contact.normal), contact.normal);

        PotionEffect effect = Instantiate(((BaseIngredientType)_baseIngredientId).PotionEffectPrefab, contact.point, rotation);

        effect.Initialize(_baseIngredientId, _modifierIngredientIds, _color);
        effect.NetworkObject.Spawn();

        NetworkObject.Despawn();
    }
}
