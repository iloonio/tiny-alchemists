using UnityEngine;

//  Ingredient.cs — A physical ingredient in the world
//
//  All pickup/drop physics are handled by PlayerInteraction.
//  This script only stores identity data and the IsHeld flag.


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Ingredient : MonoBehaviour
{
    [Header("Ingredient Data")]
    public IngredientType type;

    // Set by PlayerInteraction when grabbed/released.
    // Read by Cauldron and PlantPot to reject held items.
    public bool IsHeld;

    public IngredientCategory Category => IngredientHelper.GetCategory(type);

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        Color c = type switch
        {
            IngredientType.Cloud    => new Color(0.8f, 0.8f, 1f),
            IngredientType.Object   => new Color(0.6f, 0.4f, 0.2f),
            IngredientType.Puddle   => new Color(0.2f, 0.2f, 0.8f),
            IngredientType.Fire     => Color.red,
            IngredientType.Size     => new Color(1f, 0.5f, 0f),
            IngredientType.Float    => Color.white,
            IngredientType.Bouncy   => Color.green,
            IngredientType.Magnetic => Color.magenta,
            IngredientType.Sparkle  => Color.yellow,
            _ => Color.gray
        };

    rend.material.color = c;
    }
}
