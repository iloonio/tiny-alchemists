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
    public Transform PlayerCamera => _playerCamera;

    [Header("Throw")]
    [SerializeField] private float _throwForce = 5f;

    private Holdable _held;
    public bool IsHolding => _held != null;
    private Collider _heldCollider;
    private Collider _collider;

    private PlayerUI _playerUI;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _playerUI = GetComponent<PlayerUI>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        UpdateHint();
    }

    private void UpdateHint()
    {
        if (IsHolding)
        {
            _playerUI.Hide();
            return;
        }

        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward,
                out RaycastHit hit, _interactDistance, _interactLayer))
        {
            _playerUI.Hide();
            return;
        }

        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {
            // Use DisplayName if set, otherwise fall back to cleaned GameObject name
            string objectName = !string.IsNullOrEmpty(holdable.DisplayName)
                ? holdable.DisplayName
                : holdable.gameObject.name.Replace("(Clone)", "").Trim();

            string hint = holdable.GetComponent<Ingredient>() != null
                ? $"[LMB] Pick up {objectName} (ingredient)"
                : $"[LMB] Pick up {objectName}";

            _playerUI.Show(hint);
        }
        else
        {
            _playerUI.Hide();
        }
    }

    private void OnInteract()
    {
        if (IsHolding)
            Drop();
        else
            TryInteract();
    }

    private void OnThrow()
    {
        if (IsHolding)
            Toss();
    }

    private void Drop()
    {
        Physics.IgnoreCollision(_collider, _heldCollider, false);
        RemoveOwnershipServerRpc(_held.NetworkObject, NetworkObject);
        _held = null;
        _heldCollider = null;
    }
    
    private void Toss()
    {
        ThrowServerRpc(_held.NetworkObject, _playerCamera.forward * _throwForce);
        Physics.IgnoreCollision(_collider, _heldCollider, false);
        RemoveOwnershipServerRpc(_held.NetworkObject, NetworkObject);
        _held = null;
        _heldCollider = null;
    }

    private void TryInteract()
    {
        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward, out RaycastHit hit, _interactDistance, _interactLayer)) return;

        if (hit.collider.TryGetComponent(out IInteractable interactable))
        {        
            Interact(interactable);
        }
        else if (hit.collider.TryGetComponent(out Holdable holdable)
            && !holdable.IsHeld)
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
        Physics.IgnoreCollision(_collider, _heldCollider, true);
    }

    [ServerRpc]
    public void AcquireOwnershipServerRpc(NetworkObjectReference targetNetObjRef, NetworkObjectReference playerNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj) && playerNetObjRef.TryGet(out NetworkObject playerNetObj))
        {
            var interact = playerNetObj.GetComponent<PlayerInteract>();
            targetNetObj.GetComponent<Holdable>().PickUp(interact.HoldPoint, interact.PlayerCamera);
            Physics.IgnoreCollision(playerNetObj.GetComponent<Collider>(), targetNetObj.GetComponent<Collider>(), true);
        }
    }

    [ServerRpc]
    public void RemoveOwnershipServerRpc(NetworkObjectReference targetNetObjRef, NetworkObjectReference playerNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj) && playerNetObjRef.TryGet(out NetworkObject playerNetObj))
        {
            Physics.IgnoreCollision(playerNetObj.GetComponent<Collider>(), targetNetObj.GetComponent<Collider>(), false);
            targetNetObj.GetComponent<Holdable>().Drop();
        }
    }

    [ServerRpc]
    public void ThrowServerRpc(NetworkObjectReference targetNetObjRef, Vector3 throwForce)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
            targetNetObj.GetComponent<Holdable>().Toss(throwForce);
    }
}