using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(StatusEffectManager))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The location for taking the item (placed under the camera object)")]
    [SerializeField] private Transform holdPoint;

    [Tooltip("The player's first-person camera (used for ray detection)")]
    [SerializeField] private Transform playerCamera;

    [Header("Pick-up & Throw")]
    [Tooltip("How long is your arm (how far can you grab something)?")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float throwUpForce = 3f;

    private InputAction _interactAction;
    private Ingredient _heldIngredient;
    private Potion _heldPotion;

    // ── Status Effect Integration ──
    private StatusEffectManager _status;

    public bool IsHolding => _heldIngredient != null || _heldPotion != null;

    void Start()
    {
        _status = GetComponent<StatusEffectManager>();

        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        if(!_interactAction.WasPressedThisFrame()) return;

        // ── Block all interaction while crystallized ──
        if (_status != null && _status.IsCrystallized) return; 

        if (IsHolding)
            ThrowItem();
        else
            TryPickUp();
    }

/*
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // ── Block all interaction while crystallized ──
        if (_status != null && _status.IsCrystallized) return;

        if (IsHolding)
            ThrowItem();
        else
            TryPickUp();
    }
    */

    private void TryPickUp()
    {
        if (playerCamera == null)
        {
            Debug.LogError("In the Inspector panel, drag PlayerCamera to PlayerInteraction!");
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            if (hit.collider.CompareTag("Ingredient"))
            {
                Ingredient ing = hit.collider.GetComponent<Ingredient>();
                if (ing != null && !ing.IsHeld) GrabIngredient(ing);
            }
            else if (hit.collider.CompareTag("Potion"))
            {
                Potion pot = hit.collider.GetComponent<Potion>();
                if (pot != null) GrabPotion(pot);
            }
        }
    }

    private void GrabIngredient(Ingredient item)
    {
        _heldIngredient = item;
        _heldIngredient.OnPickedUp(holdPoint);
    }

    private void GrabPotion(Potion pot)
    {
        _heldPotion = pot;

        Rigidbody potRb = pot.GetComponent<Rigidbody>();
        Collider potCol = pot.GetComponent<Collider>();

        potRb.isKinematic = true;
        potCol.enabled = false;

        pot.transform.SetParent(holdPoint);
        pot.transform.localPosition = Vector3.zero;
        pot.transform.localRotation = Quaternion.identity;
    }

    private void ThrowItem()
    {
        Vector3 force = playerCamera.forward * throwForce + Vector3.up * throwUpForce;

        if (_heldIngredient != null)
        {
            _heldIngredient.OnDropped(force);
            _heldIngredient = null;
        }
        else if (_heldPotion != null)
        {
            _heldPotion.transform.SetParent(null);

            Rigidbody potRb = _heldPotion.GetComponent<Rigidbody>();
            Collider potCol = _heldPotion.GetComponent<Collider>();

            potRb.isKinematic = false;
            potCol.enabled = true;
            potRb.linearVelocity = Vector3.zero;
            potRb.AddForce(force, ForceMode.Impulse);

            _heldPotion.OnPickedUp();
            _heldPotion = null;
        }
    }
}
