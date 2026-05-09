using Unity.Netcode;
using UnityEngine;

public class Ingredient : NetworkBehaviour
{
    [SerializeField] private IngredientType _type;
    public IngredientType Type => _type;

}