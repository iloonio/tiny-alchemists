using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

// https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/basics/scenemanagement/using-networkscenemanager.html

public class NetworkSceneManager : NetworkBehaviour
{

    #if UNITY_EDITOR
    public UnityEditor.SceneAsset SceneAsset;
    private void OnValidate()
    {
        if (SceneAsset != null)
        {
            _sceneName = SceneAsset.name;
        }
    }
    #endif

    [SerializeField] private string _sceneName;
    [SerializeField] private GameObject _playerPrefab;
    private SpawnPoint[] _spawnPoints;
    private int _spawnPointIndex = 0;

    private void Awake() 
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {   
            NetworkManager.SceneManager.OnLoadComplete += OnClientLoadedScene;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.SceneManager.OnLoadComplete -= OnClientLoadedScene;
        }
    }

    public void ChangeScene()
    {
        if (IsServer && !string.IsNullOrEmpty(_sceneName))
        {
            var status = NetworkManager.SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {_sceneName} " 
                + $"with a {nameof(SceneEventProgressStatus)}: {status}");
            } 
            else
            {
                Debug.Log("Successfully loaded into scene");
            }
        }
    }

    private void OnClientLoadedScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == _sceneName)
        {
            StartCoroutine(SpawnPlayer(clientId));
        }
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        yield return new WaitForSeconds(2);

        if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) yield break;

        if (_spawnPoints == null)
        {
            _spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        }

        SpawnPoint spawnPoint = _spawnPoints[_spawnPointIndex];
        _spawnPointIndex = (_spawnPointIndex + 1) % _spawnPoints.Length;

        Vector3 spawnPosition = spawnPoint.Position;
        Quaternion spawnRotation = spawnPoint.Rotation;

        GameObject playerInstance = Instantiate(_playerPrefab, spawnPosition, spawnRotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned player prefab for Client ID: {clientId}");
    }
}
