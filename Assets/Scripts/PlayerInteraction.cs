using UnityEngine;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════════════════════════
//  PlayerInteraction.cs — Pickup / carry / throw
//
//  CARRY FIX (no jitter):
//    While held, the object is set kinematic and its position
//    is updated in LateUpdate (after camera moves), so it
//    perfectly tracks the holdPoint with zero visual lag.
//
//    Wall collision is handled via SphereCast: if the hold
//    position is inside a wall, we pull the object back to
//    the nearest valid point. This gives the "physics feel"
//    without actual rigidbody jitter.
//
//    On throw, kinematic is turned off and gravity + impulse
//    are applied — the object becomes a real physics body again.
// ═══════════════════════════════════════════════════════════════

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform playerCamera;

    [Header("Carry")]
    [Tooltip("Radius used for wall-avoidance SphereCast")]
    [SerializeField] private float carryCollisionRadius = 0.25f;

    [Header("Throw")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float throwUpForce = 1f;

    // ── What we're holding ──
    private enum HeldType { None, Ingredient, Potion, PlantPot }
    private HeldType _heldType = HeldType.None;
    private GameObject _heldObject;
    private Rigidbody _heldRb;

    // Type-specific references
    private Ingredient _heldIngredient;
    private Potion _heldPotion;
    private PlantPot _heldPot;

    // Saved state to restore on drop
    private bool _savedGravity;
    private float _savedDrag;
    private float _savedAngularDrag;
    private CollisionDetectionMode _savedCollisionMode;
    private RigidbodyInterpolation _savedInterpolation;

    private StatusEffectManager _status;
    private Collider _playerCollider;

    public bool IsHolding => _heldType != HeldType.None;

    void Awake()
    {
        _status = GetComponent<StatusEffectManager>();
        _playerCollider = GetComponent<Collider>();
    }

    // ──────────────────────────────────────────────
    //  INPUT
    // ──────────────────────────────────────────────

    void Update()
    {
        var input = InputManager.Instance;
        if (input == null) return;

        if (input.InteractAction.WasPressedThisFrame())
            OnInteract();
    }

    // ──────────────────────────────────────────────
    //  CARRY POSITION (runs after camera, zero jitter)
    // ──────────────────────────────────────────────

    void LateUpdate()
    {
        if (_heldRb == null) return;

        if (_heldObject == null)
        {
            ForceRelease();
            return;
        }

        Vector3 targetPos = holdPoint.position;

        // Wall avoidance: SphereCast from camera toward holdPoint
        Vector3 camPos = playerCamera.position;
        Vector3 toTarget = targetPos - camPos;
        float dist = toTarget.magnitude;

        if (dist > 0.01f && Physics.SphereCast(camPos, carryCollisionRadius,
                toTarget.normalized, out RaycastHit wallHit, dist,
                ~0, QueryTriggerInteraction.Ignore))
        {
            // Don't count hitting the held object itself
            if (wallHit.collider.gameObject != _heldObject)
            {
                // Pull back to just before the wall
                float safeDist = Mathf.Max(wallHit.distance - carryCollisionRadius, 0.1f);
                targetPos = camPos + toTarget.normalized * safeDist;
            }
        }

        // Directly place the object (kinematic, so no physics interference)
        _heldRb.position = targetPos;
        _heldObject.transform.position = targetPos;

        // Align rotation to camera forward
        Quaternion targetRot = Quaternion.LookRotation(playerCamera.forward, Vector3.up);
        _heldRb.rotation = targetRot;
        _heldObject.transform.rotation = targetRot;
    }

    // ──────────────────────────────────────────────
    //  INTERACT DISPATCH
    // ──────────────────────────────────────────────

    private void OnInteract()
    {
        if (_status != null && _status.IsCrystallized) return;

        if (IsHolding)
            ThrowHeldObject();
        else
            TryInteract();
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupDistance)) return;

        GameObject target = hit.collider.gameObject;

        // Cauldron: manual brew
        if (target.CompareTag("Cauldron"))
        {
            Cauldron cauldron = target.GetComponent<Cauldron>();
            if (cauldron == null) cauldron = target.GetComponentInParent<Cauldron>();
            if (cauldron != null) { cauldron.Brew(); return; }
        }

        // PlantPot: harvest if grown, otherwise pick up
        if (target.CompareTag("PlantPot"))
        {
            PlantPot pot = target.GetComponent<PlantPot>();
            if (pot != null)
            {
                if (pot.State == PlantPot.PotState.Grown) { pot.TryHarvest(); return; }
                Grab(target, HeldType.PlantPot);
                return;
            }
        }

        // Ingredient
        if (target.CompareTag("Ingredient"))
        {
            Ingredient ing = target.GetComponent<Ingredient>();
            if (ing != null && !ing.IsHeld)
            {
                Grab(target, HeldType.Ingredient);
                return;
            }
        }

        // Potion
        if (target.CompareTag("Potion"))
        {
            Grab(target, HeldType.Potion);
            return;
        }
    }

    // ──────────────────────────────────────────────
    //  GRAB
    // ──────────────────────────────────────────────

    private void Grab(GameObject obj, HeldType type)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        _heldObject = obj;
        _heldRb = rb;
        _heldType = type;

        _heldIngredient = (type == HeldType.Ingredient) ? obj.GetComponent<Ingredient>() : null;
        _heldPotion = (type == HeldType.Potion) ? obj.GetComponent<Potion>() : null;
        _heldPot = (type == HeldType.PlantPot) ? obj.GetComponent<PlantPot>() : null;

        // Save physics state
        _savedGravity = rb.useGravity;
        _savedDrag = rb.linearDamping;
        _savedAngularDrag = rb.angularDamping;
        _savedCollisionMode = rb.collisionDetectionMode;
        _savedInterpolation = rb.interpolation;

        // Kinematic carry: no physics simulation, we control position directly
        rb.isKinematic = true;

        // Notify the object
        if (_heldIngredient != null) _heldIngredient.IsHeld = true;
        if (_heldPot != null) _heldPot.SetHeld(true);

        // Ignore collision between player and held object
        Collider heldCol = obj.GetComponent<Collider>();
        if (heldCol != null && _playerCollider != null)
            Physics.IgnoreCollision(_playerCollider, heldCol, true);
    }

    // ──────────────────────────────────────────────
    //  THROW
    // ──────────────────────────────────────────────

    private void ThrowHeldObject()
    {
        if (_heldRb == null || _heldObject == null)
        {
            ForceRelease();
            return;
        }

        // Restore collision
        Collider heldCol = _heldObject.GetComponent<Collider>();
        if (heldCol != null && _playerCollider != null)
            Physics.IgnoreCollision(_playerCollider, heldCol, false);

        // Restore to dynamic physics body
        _heldRb.isKinematic = false;
        _heldRb.linearDamping = _savedDrag;
        _heldRb.angularDamping = _savedAngularDrag;
        _heldRb.collisionDetectionMode = _savedCollisionMode;
        _heldRb.interpolation = _savedInterpolation;
        _heldRb.useGravity = true;

        // Throw
        Vector3 force = playerCamera.forward * throwForce + Vector3.up * throwUpForce;
        _heldRb.linearVelocity = Vector3.zero;
        _heldRb.AddForce(force, ForceMode.Impulse);

        // Notify
        if (_heldIngredient != null) _heldIngredient.IsHeld = false;
        if (_heldPot != null) _heldPot.SetHeld(false);

        ForceRelease();
    }

    private void ForceRelease()
    {
        _heldObject = null;
        _heldRb = null;
        _heldType = HeldType.None;
        _heldIngredient = null;
        _heldPotion = null;
        _heldPot = null;
    }
}
