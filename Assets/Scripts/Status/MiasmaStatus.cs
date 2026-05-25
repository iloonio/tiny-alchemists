using UnityEngine;

[CreateAssetMenu(fileName = "MiasmaStatus", menuName = "ScriptableObjects/Status/MiasmaStatus")]
public class MiasmaStatus : Status
{
    [SerializeField] private float _moveSpeedMultiplierDecreasePerSecond = 0.1f;
    [SerializeField] private float _moveSpeedMultiplierMin = 0.1f;
    [SerializeField] private float _moveSpeedMultiplierMax = 0.5f;
    [SerializeField] private float _jumpForceMultiplierDecreasePerSecond = 0.1f;
    [SerializeField] private float _jumpForceMultiplierMin = 0.1f;
    [SerializeField] private float _jumpForceMultiplierMax = 0.5f;
    [SerializeField] private Status _fireStatus;

    public override void OnStatusFixedUpdate(GameObject target)
    {
        if (target.GetComponent<StatusAffectable>().HasStatus(_fireStatus)) return;

        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            if (playerMove.MoveSpeedMultiplier > _moveSpeedMultiplierMin)
            {            
                playerMove.MoveSpeedMultiplier -= _moveSpeedMultiplierDecreasePerSecond * Time.fixedDeltaTime;
                playerMove.MoveSpeedMultiplier = Mathf.Min(playerMove.MoveSpeedMultiplier, _moveSpeedMultiplierMax);
            }
            if (playerMove.JumpForceMultiplier > _jumpForceMultiplierMin)
            {
                playerMove.JumpForceMultiplier -= _jumpForceMultiplierDecreasePerSecond * Time.fixedDeltaTime;
                playerMove.JumpForceMultiplier = Mathf.Min(playerMove.JumpForceMultiplier, _jumpForceMultiplierMax);
            }
        }
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.JumpForceMultiplier = 1f;

            if (!target.GetComponent<StatusAffectable>().HasStatus(_fireStatus)) 
            {
                playerMove.MoveSpeedMultiplier = 1f;
            }
        }
    }
}