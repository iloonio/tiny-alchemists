using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(PlayerLook))]
[RequireComponent(typeof(PlayerInteract))]
public class NetworkClient : NetworkBehaviour
{
    public static readonly HashSet<NetworkClient> Players = new();

    public NetworkVariable<float> LookPitch = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private PlayerInput _playerInput;
    private PlayerMove _playerMovement;
    private PlayerLook _playerLook;
    private PlayerPush _playerPush;
    private PlayerInteract _playerInteract;
    private Camera _playerCamera;
    private AudioListener _audioListener;
    private PlayerUI _playerUI;
    private Camera _uiCamera;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerMovement = GetComponent<PlayerMove>();
        _playerLook = GetComponent<PlayerLook>();
        _playerPush = GetComponent<PlayerPush>();
        _playerInteract = GetComponent<PlayerInteract>();
        _playerCamera = GetComponentInChildren<Camera>();
        _audioListener = GetComponentInChildren<AudioListener>();
        _playerUI = GetComponent<PlayerUI>();
        _uiCamera = _playerCamera.gameObject.GetComponentInChildren<Camera>();

        _playerInput.enabled = false;
        _playerMovement.enabled = false;
        _playerLook.enabled = false;
        _playerPush.enabled = false;
        _playerInteract.enabled = false;
        _playerCamera.enabled = false;
        _audioListener.enabled = false;
        _playerUI.enabled = false;
        _uiCamera.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        Players.Add(this);

        if (IsOwner)
        {
            _playerInput.enabled = true;
            _playerMovement.enabled = true;
            _playerLook.enabled = true;
            _playerPush.enabled = true;
            _playerInteract.enabled = true;
            _playerCamera.enabled = true;
            _audioListener.enabled = true;
            _playerUI.enabled = true;
            _uiCamera.enabled = true;
        }

        if (IsServer)
        {
            LookPitch.OnValueChanged += OnLookPitchChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        Players.Remove(this);

        if (IsServer)
        {
            LookPitch.OnValueChanged -= OnLookPitchChanged;
        }
    }

    private void OnLookPitchChanged(float previous, float current)
    {
        _playerCamera.gameObject.transform.localRotation = Quaternion.Euler(current, 0f, 0f);
    }

}
