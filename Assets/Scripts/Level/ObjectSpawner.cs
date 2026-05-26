using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class ObjectSpawner : NetworkBehaviour
{
    
    [SerializeField] private NetworkObject _objectPrefab;
    [SerializeField] private float _secondsBetweenSpawns = 30f;
    private HashSet<Collider> _inside = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnObjects());
        }
    }

    private IEnumerator SpawnObjects()
    {
        while (true)
        {    
            if (_inside.Count == 0) {
                NetworkObject networkObject = Instantiate(_objectPrefab, transform.position, transform.rotation);
                networkObject.Spawn();
            }

            yield return new WaitForSeconds(_secondsBetweenSpawns);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        _inside.Add(collider);
    }

    private void OnTriggerExit(Collider collider)
    {
        _inside.Remove(collider);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (_objectPrefab != null && _objectPrefab.TryGetComponent(out Ingredient ingredient))
        {
            Gizmos.color = ingredient.Type.Color;
        }
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}