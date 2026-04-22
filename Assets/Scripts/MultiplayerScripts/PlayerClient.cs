using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class PlayerClient : NetworkBehaviour
{
    [SerializeField] private PlayerMovementFPS playerMovementFPS;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerAudioListener;
    [SerializeField] private EventSystem playerEventSystem;

    private void Awake()
    {
        playerMovementFPS.enabled = false;
        playerInteraction.enabled = false;
        playerCamera.enabled = false;
        playerAudioListener.enabled = false;
        playerEventSystem.enabled = false;
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // This is the local player, so we can enable the PlayerController script.
            playerMovementFPS.enabled = true;
            playerInteraction.enabled = true;
            playerCamera.enabled = true;
            playerAudioListener.enabled = true;
            playerEventSystem.enabled = true;
        }
        else
        {
            // This is a remote player, so we can disable the PlayerController script.
            playerMovementFPS.enabled = false;
            playerInteraction.enabled = false;
            playerCamera.enabled = false;
            playerAudioListener.enabled = false;
            playerEventSystem.enabled = false;
        }
    }
}
