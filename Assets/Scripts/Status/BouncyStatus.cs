using UnityEngine;

[CreateAssetMenu(fileName = "BouncyStatus", menuName = "ScriptableObjects/Status/BouncyStatus")]
public class BouncyStatus : Status
{
    [SerializeField] private float _bounciness = 0.8f;

    public override void OnStatusStart(GameObject target)
    {
        if (target.TryGetComponent(out Collider col))
        {
            PhysicsMaterial mat = new PhysicsMaterial("Bouncy")
            {
                bounciness = _bounciness,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            col.material = mat;

            target.GetComponentInChildren<ParticlePlayer>().Play("BounceFX");
        }
    }

    public override void OnStatusEnd(GameObject target)
    {
        if (target.TryGetComponent(out Collider col)) 
        {
            col.material = null;
            target.GetComponentInChildren<ParticlePlayer>().Stop("BounceFX");
        }
    }
}