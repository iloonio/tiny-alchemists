using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerClient : NetworkBehaviour
{
    [SerializeField] private PlayerMovementFPS playerMovementFPS;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerAudioListener;

    private void Awake()
    {
        playerMovementFPS.enabled = false;
        playerCamera.enabled = false;
        playerAudioListener.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // This is the local player, so we can enable the PlayerController script.
            playerMovementFPS.enabled = true;
            playerCamera.enabled = true;
            playerAudioListener.enabled = true;
        }
        else
        {
            // This is a remote player, so we can disable the PlayerController script.
            playerMovementFPS.enabled = false;
            playerCamera.enabled = false;
            playerAudioListener.enabled = false;
        }
    }
}
