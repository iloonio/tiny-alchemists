using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [Tooltip("拿东西的位置 (放在相机子物体下)")]
    [SerializeField] private Transform holdPoint;

    [Tooltip("玩家的第一人称相机 (用于射线检测)")]
    [SerializeField] private Transform playerCamera;

    [Header("Pick-up & Throw")]
    [Tooltip("手有多长 (能抓多远的东西)")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float throwUpForce = 3f;


    private Ingredient _heldIngredient;
    private Potion _heldPotion;

    public bool IsHolding => _heldIngredient != null || _heldPotion != null;

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (IsHolding)
            ThrowItem();
        else
            TryPickUp();
    }

    private void TryPickUp()
    {
        if (playerCamera == null)
        {
            Debug.LogError("请在 Inspector 面板中把 PlayerCamera 拖给 PlayerInteraction!");
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