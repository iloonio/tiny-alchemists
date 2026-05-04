using UnityEngine;
using Unity.Netcode;
// Very simple Unity message script to check if both players execute this script. 
public class CollisionLogger : NetworkBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) Debug.Log("I am a client! most likely.");

        Debug.Log("Collided with: " + collision);
    }
}
