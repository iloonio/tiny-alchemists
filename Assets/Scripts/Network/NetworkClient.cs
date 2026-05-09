using Unity.Netcode;
using UnityEngine.InputSystem;

public class NetworkClient : NetworkBehaviour
{
    private PlayerInput _playerInput;
    private PlayerMove _playerMovement;
    private PlayerLook _playerLook;
    private PlayerPush _playerPush;
    private PlayerInteract _playerInteract;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMove>();
        _playerLook = GetComponent<PlayerLook>();
        _playerPush = GetComponent<PlayerPush>();
        _playerInteract = GetComponent<PlayerInteract>();

        _playerInput.enabled = false;
        _playerMovement.enabled = false;
        _playerLook.enabled = false;
        _playerPush.enabled = false;
        _playerInteract.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _playerInput.enabled = true;
            _playerMovement.enabled = true;
            _playerLook.enabled = true;
            _playerPush.enabled = true;
            _playerInteract.enabled = true;
        }
    }
}
