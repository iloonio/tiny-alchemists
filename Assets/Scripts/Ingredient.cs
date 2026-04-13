using UnityEngine;

//  Ingredient.cs — A physical ingredient
//    - Tag as "Ingredient"
//    - Set 'type' in Inspector (Cloud, Object, Puddle, Fire, etc.)

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Data")]
    [Tooltip("Which ingredient is this? Determines Base vs Modifier automatically.")]
    public IngredientType type;

    public bool IsHeld { get; private set; }
    
    public IngredientCategory Category => IngredientHelper.GetCategory(type);

    private Rigidbody _rb;
    private Collider _col;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    public void OnPickedUp(Transform holdPoint)
    {
        IsHeld = true;
        _rb.isKinematic = true;
        _col.enabled = false;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDropped(Vector3 throwForce)
    {
        IsHeld = false;
        transform.SetParent(null);
        _rb.isKinematic = false;
        _col.enabled = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(throwForce, ForceMode.Impulse);
    }
}
