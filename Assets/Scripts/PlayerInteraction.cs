using UnityEngine;
using UnityEngine.InputSystem;

//  Supports:
//    - Ingredients  (tag "Ingredient")  → pick up / throw
//    - Potions      (tag "Potion")      → pick up / throw
//    - Plant Pots   (tag "PlantPot")    → pick up / throw / harvest
//    - Cauldron     (tag "Cauldron")    → manual brew

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform playerCamera;

    [Header("Pick-up & Throw")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float throwUpForce = 1f;

    // Currently held object (only one at a time)
    private Ingredient _heldIngredient;
    private Potion _heldPotion;
    private PlantPot _heldPot;

    // Status Effect Integration
    private StatusEffectManager _status;

    public bool IsHolding => _heldIngredient != null || _heldPotion != null || _heldPot != null;

    void Awake()
    {
        _status = GetComponent<StatusEffectManager>();
    }

    // Polls InputManager every frame
    void Update()
    {
        var input = InputManager.Instance;
        if (input == null) return;

        if (input.InteractAction.WasPressedThisFrame())
            OnInteract();
    }

    private void OnInteract()
    {
        if (_status != null && _status.IsCrystallized) return;

        if (IsHolding)
            ThrowItem();
        else
            TryInteract();
    }

    //  INTERACT (not holding anything)
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
            if (cauldron != null)
            {
                cauldron.Brew();
                return;
            }
        }

        //  PlantPot: harvest if grown, otherwise pick up 
        if (target.CompareTag("PlantPot"))
        {
            PlantPot pot = target.GetComponent<PlantPot>();
            if (pot != null)
            {
                if (pot.State == PlantPot.PotState.Grown)
                {
                    pot.TryHarvest();
                    return;
                }
                GrabPlantPot(pot);
                return;
            }
        }

        // Ingredient 
        if (target.CompareTag("Ingredient"))
        {
            Ingredient ing = target.GetComponent<Ingredient>();
            if (ing != null && !ing.IsHeld)
            {
                GrabIngredient(ing);
                return;
            }
        }

        // Potion
        if (target.CompareTag("Potion"))
        {
            Potion pot = target.GetComponent<Potion>();
            if (pot != null)
            {
                GrabPotion(pot);
                return;
            }
        }
    }


    //  GRAB METHODS
    private void GrabIngredient(Ingredient item)
    {
        _heldIngredient = item;
        _heldIngredient.OnPickedUp(holdPoint);
    }

    private void GrabPotion(Potion pot)
    {
        _heldPotion = pot;

        Rigidbody rb = pot.GetComponent<Rigidbody>();
        Collider col = pot.GetComponent<Collider>();
        rb.isKinematic = true;
        col.enabled = false;

        pot.transform.SetParent(holdPoint);
        pot.transform.localPosition = Vector3.zero;
        pot.transform.localRotation = Quaternion.identity;
    }

    private void GrabPlantPot(PlantPot pot)
    {
        _heldPot = pot;
        _heldPot.OnPickedUp(holdPoint);
    }
    
    //  THROW
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

            Rigidbody rb = _heldPotion.GetComponent<Rigidbody>();
            Collider col = _heldPotion.GetComponent<Collider>();
            rb.isKinematic = false;
            col.enabled = true;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);

            _heldPotion.OnPickedUp(); // enables gravity
            _heldPotion = null;
        }
        else if (_heldPot != null)
        {
            _heldPot.OnDropped(force);
            _heldPot = null;
        }
    }
}
