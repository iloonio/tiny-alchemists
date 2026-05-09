using UnityEngine;
using Unity.Netcode;

public class ObjectSpawner : NetworkBehaviour
{
    
    [SerializeField] private NetworkObject _objectPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkObject networkObject = Instantiate(_objectPrefab, transform.position, transform.rotation);
            networkObject.Spawn();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (_objectPrefab != null )
        {
            Renderer renderer = _objectPrefab.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Gizmos.color = renderer.sharedMaterial.color;
            }
        }
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}