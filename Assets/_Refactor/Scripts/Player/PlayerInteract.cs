using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
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
    private Collider _heldCollider;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

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
        Physics.IgnoreCollision(_collider, _heldCollider, false);
        RemoveOwnershipRpc(_held.NetworkObject);
        _held = null;
        _heldCollider = null;
    }
    
    private void Toss()
    {
        _held.Toss(_playerCamera.forward * _throwForce);
        Drop();
    }

    private void TryInteract()
    {
        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward, out RaycastHit hit, _interactDistance, _interactLayer)) return;
        Debug.Log("Hit " + hit.collider.gameObject);

        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {        
            Debug.Log("Interacting with" + hit.collider.gameObject);
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
        _held.PickUp();
        AcquireOwnershipRpc(_held.NetworkObject);
        Physics.IgnoreCollision(_collider, _heldCollider, true);
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
