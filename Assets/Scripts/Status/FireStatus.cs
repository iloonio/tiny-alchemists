using UnityEngine;

[CreateAssetMenu(fileName = "FireStatus", menuName = "ScriptableObjects/Status/FireStatus")]
public class FireStatus : Status
{
    [SerializeField] private float _speedMultiplier = 1.5f;

    public override void OnStart(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MultiplyMoveSpeed(_speedMultiplier);
        }
    }

    public override void OnEnd(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MultiplyMoveSpeed(1f / _speedMultiplier);
        }
    }
}