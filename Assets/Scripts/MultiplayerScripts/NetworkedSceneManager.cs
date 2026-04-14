using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


// https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/basics/scenemanagement/using-networkscenemanager.html

public class NetworkedSceneManager : NetworkBehaviour
{
    /// INFO: You can remove the #if UNITY_EDITOR code segment and make SceneName public,
    /// but this code assures if the scene name changes you won't have to remember to
    /// manually update it.
    #if UNITY_EDITOR
    public UnityEditor.SceneAsset SceneAsset;
    private void OnValidate()
    {
        if (SceneAsset != null)
        {
            m_SceneName = SceneAsset.name;
        }
    }
    #endif

    [SerializeField] private string m_SceneName;
    [SerializeField] private GameObject m_playerPrefab;

    private void Awake() 
    {
        DontDestroyOnLoad(gameObject); //Keep this object around after scene change
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
        if (IsServer && !string.IsNullOrEmpty(m_SceneName))
        {
            var status = NetworkManager.SceneManager.LoadScene(m_SceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {m_SceneName} " 
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
        if (sceneName == m_SceneName)
        {
            // instantiate the prefab on the server
            StartCoroutine(SpawnPlayer(clientId));
        }
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        yield return new WaitForSeconds(2);

        if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) yield break;

        SpawnPoint[] spawnPoints = UnityEngine.Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        // determine spawn pos for client. We can have multiple spawnpoints on the map. 
        if (spawnPoints.Length > 0)
        {
            int index = (int)clientId % spawnPoints.Length;
            spawnPos = spawnPoints[index].transform.position;
            spawnRot = spawnPoints[index].transform.rotation;
        }

        GameObject playerInstance = Instantiate(m_playerPrefab, spawnPos, spawnRot);
        // spawn the object across the network
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);

        Debug.Log($"Spawned player prefab for Client ID: {clientId}");
    }
}
