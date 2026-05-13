using UnityEngine;

[CreateAssetMenu(fileName = "FireStatus", menuName = "ScriptableObjects/Status/FireStatus")]
public class FireStatus : Status
{
    [SerializeField] private float _speedMultiplier = 1.5f;

    public override void OnStatusStart(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MoveSpeedMultiplier *= _speedMultiplier;
        }
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MoveSpeedMultiplier *= 1f / _speedMultiplier;
        }
    }
}