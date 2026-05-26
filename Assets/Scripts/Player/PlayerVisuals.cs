using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerVisuals : NetworkBehaviour
{

    [SerializeField] private NetworkClient _networkClient;

    [Header("Colours")]
    [SerializeField] private Renderer _hatRenderer;
    [SerializeField] private Renderer _cloakRenderer;
    [SerializeField] private List<Color> _playerColors;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        _networkClient.IsMoving.OnValueChanged += OnIsMovingChanged;
        _networkClient.IsGrounded.OnValueChanged += OnIsGroundedChanged;
        
        int playerIndex =  ((int) NetworkObject.OwnerClientId) % _playerColors.Count;
        _hatRenderer.material.color = _playerColors[playerIndex];
        _cloakRenderer.material.color = _playerColors[playerIndex];
    }

    public override void OnNetworkDespawn()
    {
        _networkClient.IsMoving.OnValueChanged -= OnIsMovingChanged;
        _networkClient.IsGrounded.OnValueChanged -= OnIsGroundedChanged;
    }

    private void OnIsMovingChanged(bool previousValue, bool newValue)
    {
        _animator.SetBool("IsWalking", newValue);
    }

    private void OnIsGroundedChanged(bool previousValue, bool newValue)
    {
        _animator.SetBool("IsAirborne", !newValue);
    }
}
