using UnityEngine;

[CreateAssetMenu(fileName = "FireStatus", menuName = "ScriptableObjects/Status/FireStatus")]
public class FireStatus : Status
{
    [SerializeField] private float _speedMultiplier = 1.5f;
    [SerializeField] private Vector2 _inputOverride = Vector2.up;

    public override void OnStatusStart(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MoveSpeedMultiplier = _speedMultiplier;
            playerMove.OverrideInput(_inputOverride);
            target.GetComponentInChildren<AudioPlayer>().Play("FireStatusLoop");
        }

        if (target.TryGetComponent(out Flammable _))
        {
            target.GetComponentInChildren<AudioPlayer>().Play("FireStatusLoop");
        }
    }

    public override void OnStatusFixedUpdate(GameObject target)
    {
        if (target.TryGetComponent(out Flammable flammable))
        {
            flammable.Burn(Time.fixedDeltaTime);
        }
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out PlayerMove playerMove))
        {
            playerMove.MoveSpeedMultiplier = 1f;
            playerMove.ClearInputOverride();
            target.GetComponentInChildren<AudioPlayer>().Stop("FireStatusLoop");
        }

        if (target.TryGetComponent(out Flammable _))
        {
            target.GetComponentInChildren<AudioPlayer>().Stop("FireStatusLoop");
        }
    }
}