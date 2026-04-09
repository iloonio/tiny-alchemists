using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;


namespace Unity.Multiplayer.Widgets
{
    [RequireComponent(typeof(Button))]
   public class StartGame : MonoBehaviour
    {
        [SerializeField] private string m_SceneToLoad;
        
        Button m_Button;

        void Start()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(GameStart);
            
            // Subscribe to connection events to toggle button state
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnNetworkStatusChanged;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnNetworkStatusChanged;
            }

            RefreshButtonState();
        }

      private void OnDestroy()
        {
            // Clean up listeners
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnNetworkStatusChanged;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnNetworkStatusChanged;
            }
        }

        private void OnNetworkStatusChanged(ulong clientId)
        {
            RefreshButtonState();
        }

        void RefreshButtonState()
        {
            if (NetworkManager.Singleton == null) return;

            // Only the Host/Server should be able to click "Start" 
            // Clients just wait for the server to switch scenes.
            bool isSessionActive = NetworkManager.Singleton.IsListening;
            bool isHost = NetworkManager.Singleton.IsServer;

            m_Button.interactable = isSessionActive && isHost;
        }

        void GameStart()
        {
            // Double check authority
            if (!NetworkManager.Singleton.IsServer) return;

            // Use NetworkSceneManager to ensure all connected clients load the scene
            var status = NetworkManager.Singleton.SceneManager.LoadScene(
                m_SceneToLoad, 
                LoadSceneMode.Single
            );

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to start scene load: {status}");
            }
        }
    }
}
// TODO: Implement a script that loads the main GameScene when pressed. 
