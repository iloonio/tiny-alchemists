using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NetworkObject))]
public class CollisionOwnershipTransfer : NetworkBehaviour
{
    private Collider objectCollider;
    private NetworkObject m_networkObject;

    private void Start()
    {
        objectCollider = GetComponent<Collider>();
        m_networkObject = GetComponent<NetworkObject>();
    }

    public void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision Detected with: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            NetworkObject networkObject = other.gameObject.GetComponentInParent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                // Change Ownership 
                Debug.Log("Transferring ownership to: " + networkObject.OwnerClientId);
                m_networkObject.ChangeOwnership(networkObject.OwnerClientId);
            }
        }
    }


}