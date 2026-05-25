using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PanaceaCauldron : NetworkBehaviour
{

    [Header("Victory Condition")]
    [SerializeField] private int _victoryIngredientCount = 5;

    [Header("Player Stuck Detection")]
    [SerializeField] private float _playerStuckTime = 3f;
    [SerializeField] private float _explosionForce = 12f;

    private List<int> _contents = new();
    private Dictionary<PlayerPush, float> _playersInside = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Ingredient ingredient)
            && !_contents.Contains((int)ingredient.Type))
        {
            _contents.Add((int)ingredient.Type);
            ingredient.NetworkObject.Despawn();

            if (_contents.Count == _victoryIngredientCount)
            {
                VictoryClientRpc();
                FindAnyObjectByType<NetworkSceneManager>().Shutdown();
            }
        }

        if (other.TryGetComponent(out PlayerPush playerPush))
        {
            _playersInside[playerPush] = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out PlayerPush playerPush))
        {
            _playersInside.Remove(playerPush);
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        LaunchStuckPlayers();
    }

    public void LaunchStuckPlayers()
    {
        if (_playersInside.Count == 0) return;

        foreach (var key in new List<PlayerPush>(_playersInside.Keys))
        {
            _playersInside[key] += Time.deltaTime;

            if (_playersInside[key] >= _playerStuckTime)
            {
                Vector3 launchForce = Vector3.up * _explosionForce;
                key.AddForceClientRpc(launchForce);
                _playersInside.Remove(key);
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void VictoryClientRpc()
    {
        foreach (NetworkClient client in NetworkClient.Players)
        {
            client.GetComponent<PlayerUI>().ShowMajor("VICTORY!");
        }
    }
}
