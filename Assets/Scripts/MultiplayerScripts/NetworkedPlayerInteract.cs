using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInteraction))]
public class NetworkedPlayerInteraction : NetworkBehaviour
{
    private InputAction _interactAction;
    private PlayerInteraction _playerInteraction;
    private Transform _playerCamera;
    private float _rayDistance;

    // TODO: implement ownership transfer with a raycast that is of the same range as the interact key. 

    void Awake()
    {
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerCamera = _playerInteraction.GetPlayerCamera();
        _rayDistance = _playerInteraction.GetPickupDistance();
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        // Only the local player should process their own input
        if (!IsOwner) return;
        // If player interacts and they are currently not holding anything 
        if (_interactAction.WasPressedThisFrame() && !_playerInteraction.IsHolding)
        {
            TryOwnerShipTransfer();
        }
    }

    public void TryOwnerShipTransfer()
    {
        Ray ray = new(_playerCamera.position, _playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, _rayDistance)) return;

        GameObject target = hit.collider.gameObject;

        // If we hit something interactable that can have its ownership transferred
        if (target.CompareTag("Potion") || target.CompareTag("Ingredient") || target.CompareTag("PlantPot"))
        {
            NetworkObject targetNetObj = target.GetComponentInParent<NetworkObject>();

            // If the object has a NetworkObject attached to it
            if (targetNetObj != null && targetNetObj.IsSpawned)
            {
                // This should work, but if it doesn't we will send RPCs instead. 
                Debug.Log("Transferring ownership to: " + NetworkObject.OwnerClientId);
                RequestOwnershipServerRpc(targetNetObj);
            }
        }
    } 

    [ServerRpc]
    private void RequestOwnershipServerRpc(NetworkObjectReference targetNetObjRef, ServerRpcParams rpcParams = default)
    {
        if (targetNetObjRef.TryGet(out NetworkObject targetNetObj))
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            Debug.Log($"Server: Transferring ownership of {targetNetObj.name} to Client {clientId}");

            targetNetObj.ChangeOwnership(clientId);
        }
    }


}