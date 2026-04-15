using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(StatusEffectManager))]
public class NetworkedPlayerInteract : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform playerCamera;

    [Header("Physics Settings")]
    [SerializeField] private float followSpeed = 25f; 
    [SerializeField] private float maxDistanceBeforeDrop = 3f;

    private NetworkObject _heldNetworkObject;
    private Rigidbody _heldRigidbody;
    private StatusEffectManager _status;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        _status = GetComponent<StatusEffectManager>();
    }

    private void FixedUpdate()
    {
        // Only the owner calculates the physics pull for their held object
        if (IsOwner && _heldNetworkObject != null && _heldRigidbody != null)
        {
            MoveObjectWithPhysics();
        }
    }

    private void MoveObjectWithPhysics()
    {
        float distance = Vector3.Distance(_heldNetworkObject.transform.position, holdPoint.position);
        
        // Auto-drop if the object gets stuck behind a wall
        if (distance > maxDistanceBeforeDrop)
        {
            RequestThrowServerRpc(Vector3.zero);
            return;
        }

        // Velocity-based follow (allows for collisions)
        Vector3 targetVelocity = (holdPoint.position - _heldNetworkObject.transform.position) * followSpeed;
        _heldRigidbody.linearVelocity = targetVelocity;

        // Rotation follow
        _heldRigidbody.angularVelocity = Vector3.zero;
        _heldNetworkObject.transform.rotation = Quaternion.Slerp(
            _heldNetworkObject.transform.rotation, 
            holdPoint.rotation, 
            Time.fixedDeltaTime * followSpeed
        );
    }

    // --- GRAB LOGIC ---

    [ServerRpc]
    public void RequestGrabServerRpc(NetworkObjectReference netObjRef)
    {
        if (netObjRef.TryGet(out NetworkObject netObj))
        {
            // 1. Give Ownership to the client so their FixedUpdate controls it
            netObj.ChangeOwnership(OwnerClientId);

            // 2. Set physics state for all clients
            SetObjectPhysicsClientRpc(netObjRef, true);

            // 3. Tell the specific client to set their local references
            UpdateLocalRefsClientRpc(netObjRef);
        }
    }

    [ClientRpc]
    private void UpdateLocalRefsClientRpc(NetworkObjectReference netObjRef)
    {
        if (!IsOwner) return;

        if (netObjRef.TryGet(out NetworkObject netObj))
        {
            _heldNetworkObject = netObj;
            _heldRigidbody = netObj.GetComponent<Rigidbody>();
        }
    }

    // --- THROW LOGIC ---

    [ServerRpc]
    public void RequestThrowServerRpc(Vector3 throwForce)
    {
        if (_heldNetworkObject == null) return;

        // 1. Reset Physics state
        SetObjectPhysicsClientRpc(_heldNetworkObject, false);

        // 2. Apply the actual throw force
        if (_heldRigidbody != null)
        {
            _heldRigidbody.AddForce(throwForce, ForceMode.Impulse);
        }

        // 3. Remove ownership and clear references
        _heldNetworkObject.RemoveOwnership();
        ClearLocalRefsClientRpc();
    }

    [ClientRpc]
    private void ClearLocalRefsClientRpc()
    {
        if (!IsOwner) return;
        _heldNetworkObject = null;
        _heldRigidbody = null;
    }

    // --- SHARED PHYSICS STATE ---

    [ClientRpc]
    private void SetObjectPhysicsClientRpc(NetworkObjectReference netObjRef, bool isHeld)
    {
        if (netObjRef.TryGet(out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();
            if (rb == null) return;

            // We keep Kinematic FALSE so it can hit walls
            rb.isKinematic = false; 
            rb.useGravity = !isHeld;
            
            // Continuous prevents the object from phasing through thin walls while moving fast
            rb.collisionDetectionMode = isHeld ? 
                CollisionDetectionMode.Continuous : 
                CollisionDetectionMode.Discrete;

            // Optional: Ignore collision with player while holding to prevent "flying"
            Collider itemCol = netObj.GetComponent<Collider>();
            Collider playerCol = GetComponent<Collider>();
            if (itemCol != null && playerCol != null)
            {
                Physics.IgnoreCollision(itemCol, playerCol, isHeld);
            }
        }
    }
}