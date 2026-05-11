using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerInteract : NetworkBehaviour
{
    [Header("Interact")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private float _interactDistance = 1f;
    [SerializeField] private LayerMask _interactLayer;

    [Header("Hold")]
    [SerializeField] private Transform _holdPoint;
    public Transform HoldPoint => _holdPoint;
    [SerializeField] private float _followPositionSpeed = 30f;
    [SerializeField] private float _followRotationSpeed = 10f;

    [Header("Throw")]
    [SerializeField] private float _throwForce = 5f;

    private Holdable _held;
    public bool IsHolding => _held != null;
    private Collider _heldCollider;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnInteract()
    {
        if (IsHolding)
        {
            Drop();
        }
        else
        {
            TryInteract();
        }
    }

    private void OnThrow()
    {
        if (IsHolding)
        {
            Toss();
        }
    }

    private void Drop()
    {
        _held.Drop();

        RemoveOwnershipServerRpc(_held.NetworkObject);
        
        _held = null;
        _heldCollider = null;
    }
    
    private void Toss()
    {
        ThrowServerRpc(_held.NetworkObject, _playerCamera.forward * _throwForce);
        Drop();
    }

    private void TryInteract()
    {
        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward, out RaycastHit hit, _interactDistance, _interactLayer)) return;

        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {        
            Interact(interactable);
        }
        else if (hit.collider.TryGetComponent(out Holdable holdable)
            && holdable.NetworkObject.IsOwnedByServer)
        {
            PickUp(holdable, hit.collider);
        }
    }

    private void Interact(IInteractable interactable)
    {
        interactable.Interact();
    }

    private void PickUp(Holdable holdable, Collider collider)
    {
        _held = holdable;
        _heldCollider = collider;

        AcquireOwnershipServerRpc(_held.NetworkObject, NetworkObject);
    }

    [ServerRpc]
    public void AcquireOwnershipServerRpc(NetworkObjectReference targetNetObjRef, NetworkObjectReference playerNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj) && playerNetObjRef.TryGet(out NetworkObject playerNetObj))
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            Debug.Log($"Server: Transferring ownership of {targetNetObj.name} to Client {clientId}");

            targetNetObj.GetComponent<Holdable>().PickUp(playerNetObj.GetComponent<PlayerInteract>().HoldPoint);
            Physics.IgnoreCollision(playerNetObj.GetComponent<Collider>(), targetNetObj.GetComponent<Collider>(), true);
        }
    }

    [ServerRpc]
    public void RemoveOwnershipServerRpc(NetworkObjectReference targetNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            Debug.Log($"Server: Relinquishing ownership of {targetNetObj.name} by Client {clientId}");

            targetNetObj.GetComponent<Holdable>().Drop();
        }
    }

    [ServerRpc]
    public void ThrowServerRpc(NetworkObjectReference targetNetObjRef, Vector3 throwForce)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
        {
            targetNetObj.GetComponent<Holdable>().Toss(throwForce);
        }
    }

}
