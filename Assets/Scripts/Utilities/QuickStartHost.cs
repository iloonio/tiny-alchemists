using UnityEngine;
using Unity.Netcode;

public class QuickStartHost : MonoBehaviour
{
    void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) return;

        if (GUI.Button(new Rect(10, 10, 200, 40), "Start Host"))
            NetworkManager.Singleton.StartHost();

        if (GUI.Button(new Rect(10, 60, 200, 40), "Start Client"))
            NetworkManager.Singleton.StartClient();
    }
}