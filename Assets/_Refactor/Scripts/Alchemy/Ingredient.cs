using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Ingredient : MonoBehaviour
{
    [SerializeField] private IngredientType _type;
    public IngredientType Type => _type;
    private NetworkObject _networkObject;
    public NetworkObject NetworkObject => _networkObject;

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
    }
}