
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(StatusAffectable))]
public class Flammable : NetworkBehaviour
{
    [SerializeField] private float durability = 2f;

    public void Burn(float duration)
    {
        if (!IsServer) return;

        durability -= duration;

        if (durability <= 0f)
        {
            NetworkObject.Despawn(gameObject);
        }
    }
}
