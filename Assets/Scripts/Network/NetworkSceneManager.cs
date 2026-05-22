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
            _gameSceneName = SceneAsset.name;
        }
    }
    #endif

    [SerializeField] private string _setupSceneName;
    [SerializeField] private string _gameSceneName;
    [SerializeField] private GameObject _playerPrefab;
    private SpawnPoint[] _spawnPoints;
    private int _spawnPointIndex = 0;
    private bool _isShutdown = false;

    private LoadingScreen _loadingScreen;

    private void Awake() 
    {
        _loadingScreen = GetComponent<LoadingScreen>();
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(_setupSceneName);
        Destroy(gameObject);
    }

    public void ChangeScene()
    {
        if (IsServer && !string.IsNullOrEmpty(_gameSceneName))
        {
            // Tell ALL clients (including host) to fade to black,
            // then the server actually loads the scene.
            FadeInClientRpc();

            StartCoroutine(LoadSceneAfterFade());
        }
    }

    private IEnumerator LoadSceneAfterFade()
    {
        // Wait for the fade-in to finish before loading
        if (_loadingScreen != null)
            yield return _loadingScreen.Transition(null);
        else
            yield return new WaitForSeconds(0.5f);

        var status = NetworkManager.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {_gameSceneName} "
                + $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
        else
        {
            Debug.Log("Successfully loaded into scene");
        }
    }

    [Rpc(SendTo.NotServer)]
    private void FadeInClientRpc()
    {
        // Non-host clients fade to black on their own
        if (_loadingScreen != null)
            StartCoroutine(_loadingScreen.Transition(null));
    }

    private void OnClientLoadedScene(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == _gameSceneName)
        {
            StartCoroutine(SpawnPlayer(clientId));

            // Tell this specific client to fade out
            FadeOutClientRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void FadeOutClientRpc(RpcParams rpcParams)
    {
        if (_loadingScreen != null)
            StartCoroutine(_loadingScreen.FadeOut());
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        yield return null;

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

    public void Shutdown()
    {
        if (!IsServer || _isShutdown) return;

        _isShutdown = true;
        _spawnPoints = null;
        _spawnPointIndex = 0;

        StartCoroutine(ShutdownRoutine());
    }

    private IEnumerator ShutdownRoutine()
    {
        yield return new WaitForSeconds(5f);

        FadeInClientRpc();

        yield return new WaitForSeconds(0.5f);

        NetworkManager.Shutdown();
    }
}