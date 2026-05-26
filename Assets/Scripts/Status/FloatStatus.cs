using UnityEngine;

[CreateAssetMenu(fileName = "FloatStatus", menuName = "ScriptableObjects/Status/FloatStatus")]
public class FloatStatus : Status
{
    public override void OnStatusStart(GameObject target)
    {
        if (target.TryGetComponent(out Rigidbody rb))
            rb.useGravity = false;

        target.GetComponentInChildren<AudioPlayer>().Play("FloatStatusLoop");
        target.GetComponentInChildren<AudioPlayer>().Play("FloatFX");
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out Rigidbody rb))
            rb.useGravity = true;
        
        target.GetComponentInChildren<AudioPlayer>().Stop("FloatStatusLoop");
        target.GetComponentInChildren<AudioPlayer>().Stop("FloatFX");
    }
}