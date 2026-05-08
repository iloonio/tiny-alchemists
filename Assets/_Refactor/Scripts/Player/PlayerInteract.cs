using Unity.Netcode;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interact")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private float _interactDistance = 1f;
    [SerializeField] private LayerMask _interactLayer;

    [Header("Hold")]
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private float _followPositionSpeed = 30f;
    [SerializeField] private float _followRotationSpeed = 10f;

    [Header("Throw")]
    [SerializeField] private float _throwForce = 5f;

    private Holdable _held;
    public bool IsHolding => _held != null;

    private void Update()
    {
        FollowHoldPoint();
    }

    private void FollowHoldPoint()
    {
        if (!IsHolding) return;
        _held.Follow(_holdPoint, _followPositionSpeed, _followRotationSpeed);
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
        RemoveOwnershipRpc(_held.NetworkObject);
        _held = null;
    }
    
    private void Toss()
    {
        _held.Toss(_playerCamera.forward * _throwForce);
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
            PickUp(holdable);
        }
    }

    private void Interact(IInteractable interactable)
    {
        interactable.Interact();
    }

    private void PickUp(Holdable holdable)
    {
        _held = holdable;
        _held.PickUp();
        AcquireOwnershipRpc(_held.NetworkObject);
    }

    [ServerRpc]
    private void AcquireOwnershipRpc(NetworkObjectReference targetNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
        {
            targetNetObj.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc]
    private void RemoveOwnershipRpc(NetworkObjectReference targetNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
        {
            targetNetObj.RemoveOwnership();
        }
    }

}
