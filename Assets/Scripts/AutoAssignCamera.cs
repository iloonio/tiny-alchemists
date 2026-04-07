
using UnityEngine;
using Unity.Cinemachine; 

public class AutoAssignCamera : MonoBehaviour
{
    void Start()
    {

        var vcam = FindAnyObjectByType<CinemachineVirtualCamera>();

        if (vcam != null)
        {

            vcam.Follow = this.transform;
            vcam.LookAt = this.transform;
        }
    }
}