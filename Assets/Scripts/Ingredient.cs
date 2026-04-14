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

    private void Start()
    {
        TintBasedOnType();
    }

    private void TintBasedOnType()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Color targetColor = Color.gray; // default grey

        switch (type)
        {
            // Base
            case IngredientType.Cloud: targetColor = new Color(0.8f, 0.8f, 1f); break; // light blue
            case IngredientType.Object: targetColor = new Color(0.6f, 0.4f, 0.2f); break; // brown
            case IngredientType.Puddle: targetColor = new Color(0.2f, 0.2f, 0.8f); break; // dark blue

            // Modifiers
            case IngredientType.Fire: targetColor = Color.red; break;
            case IngredientType.Size: targetColor = new Color(1f, 0.5f, 0f); break; // orange
            case IngredientType.Float: targetColor = Color.white; break;
            case IngredientType.Bouncy: targetColor = Color.green; break;
            case IngredientType.Magnetic: targetColor = Color.magenta; break; // magenta
            case IngredientType.Sparkle: targetColor = Color.yellow; break;
        }

        rend.material.color = targetColor;
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
