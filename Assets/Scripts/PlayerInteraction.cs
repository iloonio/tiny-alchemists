using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;



//  PHYSICS-BASED CARRY:
//    - Held objects are NOT parented to anything
//    - Held objects keep their Rigidbody and Collider active
//    - Every FixedUpdate, we set the object's velocity to fly
//      toward holdPoint — it collides with walls naturally
//    - On throw, we just apply an impulse
//
//  This means objects can't clip through walls, and there's no
//  risk of dropping items out of the map by facing a wall.

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform playerCamera;

    [Header("Carry Physics")]
    [Tooltip("How aggressively the object chases the hold point")]
    [SerializeField] private float carrySpeed = 30f;
    [Tooltip("Damping to prevent oscillation")]
    [SerializeField] private float carryDamping = 8f;

    [Header("Throw")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float throwUpForce = 1f;

    // What we are holding
    private enum HeldType { None, Ingredient, Potion, PlantPot }
    private HeldType _heldType = HeldType.None;
    private GameObject _heldObject;
    private Rigidbody _heldRb;

    // Type-specific references (for calling type-specific methods)
    private Ingredient _heldIngredient;
    private Potion _heldPotion;
    private PlantPot _heldPot;

    // Saved state to restore on drop
    private bool _savedGravity;
    private float _savedDrag;
    private float _savedAngularDrag;
    private CollisionDetectionMode _savedCollisionMode;

    private StatusEffectManager _status;

    private Collider _playerCollider;

    public bool IsHolding => _heldType != HeldType.None;

    void Start()
    {
        _status = GetComponent<StatusEffectManager>();
        _playerCollider = GetComponent<Collider>();
    }

    //  INPUT (polls InputManager each frame)
    void Update()
    {
        var input = InputManager.Instance;
        if (input == null) return;

        if (input.InteractAction.WasPressedThisFrame())
            OnInteract();
    }


    //  PHYSICS CARRY (moves held object toward holdPoint)
    void FixedUpdate()
    {
        if (_heldRb == null) return;

        // If the object got destroyed while held (e.g., burned), release
        if (_heldObject == null)
        {
            ForceRelease();
            return;
        }

        // MovePosition directly — much tighter than velocity chasing
        _heldRb.MovePosition(Vector3.Lerp(_heldRb.position, holdPoint.position, carrySpeed * Time.fixedDeltaTime));

        // Align rotation to camera
        Quaternion targetRot = Quaternion.LookRotation(playerCamera.forward, Vector3.up);
        _heldRb.MoveRotation(Quaternion.Slerp(_heldRb.rotation, targetRot, 10f * Time.fixedDeltaTime));
    }


    //  INTERACT DISPATCH
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

        // Cauldron: manual brew (not a pickup)
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


    //  GRAB — Unified for all object types
    private void Grab(GameObject obj, HeldType type)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;

        _heldObject = obj;
        _heldRb = rb;
        _heldType = type;

        // Cache component references
        _heldIngredient = (type == HeldType.Ingredient) ? obj.GetComponent<Ingredient>() : null;
        _heldPotion = (type == HeldType.Potion) ? obj.GetComponent<Potion>() : null;
        _heldPot = (type == HeldType.PlantPot) ? obj.GetComponent<PlantPot>() : null;

        // Save physics state
        _savedGravity = rb.useGravity;
        _savedDrag = rb.linearDamping;
        _savedAngularDrag = rb.angularDamping;
        _savedCollisionMode = rb.collisionDetectionMode;

        // Configure for carry: no gravity, high drag, continuous collision
        rb.useGravity = false;
        rb.linearDamping = 0f;       // we control velocity directly
        rb.angularDamping = 5f;      // stop spinning
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Notify the object it's being held
        if (_heldIngredient != null) _heldIngredient.IsHeld = true;
        if (_heldPot != null) _heldPot.SetHeld(true);
        // Potion: enable gravity flag for when it's thrown
        // (it spawns with gravity off; OnPickedUp used to flip it on)

        // Ignore collision between player and held object
        Collider heldCol = obj.GetComponent<Collider>();
        if (heldCol != null && _playerCollider != null)
            Physics.IgnoreCollision(_playerCollider, heldCol, true);
    }


    //  THROW — Unified for all object types
    private void ThrowHeldObject()
    {
        // Restore collision with player
        Collider heldCol = _heldObject.GetComponent<Collider>();
        if (heldCol != null && _playerCollider != null)
            Physics.IgnoreCollision(_playerCollider, heldCol, false);

        if (_heldRb == null || _heldObject == null)
        {
            ForceRelease();
            return;
        }

        // Restore physics state
        _heldRb.linearDamping = _savedDrag;
        _heldRb.angularDamping = _savedAngularDrag;
        _heldRb.collisionDetectionMode = _savedCollisionMode;

        // Enable gravity (always, so thrown objects fall)
        _heldRb.useGravity = true;

        // Apply throw impulse
        Vector3 force = playerCamera.forward * throwForce + Vector3.up * throwUpForce;
        _heldRb.linearVelocity = Vector3.zero;
        _heldRb.AddForce(force, ForceMode.Impulse);

        // Notify the object it's been released
        if (_heldIngredient != null) _heldIngredient.IsHeld = false;
        if (_heldPot != null) _heldPot.SetHeld(false);

        // Clear references
        ForceRelease();
    }

    // Wipe all held-object state without applying physics.
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
