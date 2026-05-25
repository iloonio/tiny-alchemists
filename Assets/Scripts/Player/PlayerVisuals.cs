using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerVisuals : NetworkBehaviour
{

    [SerializeField] private NetworkClient _networkClient;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        _networkClient.IsMoving.OnValueChanged += OnIsMovingChanged;
        _networkClient.IsGrounded.OnValueChanged += OnIsGroundedChanged;
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
