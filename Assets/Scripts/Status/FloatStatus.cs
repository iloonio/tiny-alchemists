using UnityEngine;

[CreateAssetMenu(fileName = "FloatStatus", menuName = "ScriptableObjects/Status/FloatStatus")]
public class FloatStatus : Status
{
    public override void OnStatusStart(GameObject target)
    {
        if (target.TryGetComponent(out Rigidbody rb))
            rb.useGravity = false;
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out Rigidbody rb))
            rb.useGravity = true;
    }
}