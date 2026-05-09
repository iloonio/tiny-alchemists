using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireModifier", menuName = "ScriptableObjects/IngredientType/FireModifier")]
public class FireModifierIngredientType : ModifierIngredientType
{
    public Status FireStatus;
    public float FireStatusDuration = 5f;

    public override void OnEffectStart(PotionEffect effect, NoBaseIngredientType noBaseIngredient, List<ModifierIngredientType> modifierIngredients)
    {
        foreach (var collider in Physics.OverlapSphere(effect.transform.position, noBaseIngredient.Radius))
        {
            if (collider.TryGetComponent(out StatusAffectable statusAffectable))
            {
                statusAffectable.AddStatus(FireStatus, FireStatusDuration);
            }
        }        
    }
}