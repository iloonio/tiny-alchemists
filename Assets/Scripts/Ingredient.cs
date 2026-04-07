using UnityEngine;


public enum IngredientType
{
    FireFlower,
    CrystalFlower,
    SparkleFlower
}

public enum PotionType
{
    FailedSludge,
    Fire,
    Crystal,
    Sparkle,
    Explosive,    // Fire + Sparkle
    FireCrystal   // Fire + Crystal
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Data")]
    public IngredientType type; 
    public bool IsHeld { get; private set; }

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