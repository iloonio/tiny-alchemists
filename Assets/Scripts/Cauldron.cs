using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject potionPrefab;       
    public Transform potionSpawnPoint;    

    private List<IngredientType> _contents = new List<IngredientType>();
    private Coroutine _cookTimerCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            Ingredient item = other.GetComponent<Ingredient>();

           
            if (item != null && !item.IsHeld && _contents.Count < 2)
            {
                AddIngredient(item);
            }
        }
    }

    private void AddIngredient(Ingredient item)
    {
        
        _contents.Add(item.type);
        Destroy(item.gameObject);

        
        if (_contents.Count == 2)
        {
            if (_cookTimerCoroutine != null) StopCoroutine(_cookTimerCoroutine);
            BrewPotion();
        }
        
        else if (_contents.Count == 1)
        {
            _cookTimerCoroutine = StartCoroutine(CookTimerRoutine());
        }
    }

    private IEnumerator CookTimerRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        BrewPotion();
    }

    private void BrewPotion()
    {
        
        PotionType result = DetermineRecipe(_contents);

        Debug.Log($"<color=cyan>[Alchemy]</color> Brewed a {result} Potion!");

        
        GameObject newPotion = Instantiate(potionPrefab, potionSpawnPoint.position, Quaternion.identity);

        
        newPotion.GetComponent<Potion>().Initialize(result);

        
        _contents.Clear();
    }

    private PotionType DetermineRecipe(List<IngredientType> ingredients)
    {
        if (ingredients.Count == 1)
        {
            if (ingredients[0] == IngredientType.FireFlower) return PotionType.Fire;
            if (ingredients[0] == IngredientType.CrystalFlower) return PotionType.Crystal;
            if (ingredients[0] == IngredientType.SparkleFlower) return PotionType.Sparkle;
        }
        else if (ingredients.Count == 2)
        {
            
            if (ingredients.Contains(IngredientType.FireFlower) && ingredients.Contains(IngredientType.SparkleFlower))
                return PotionType.Explosive;

            
            if (ingredients.Contains(IngredientType.FireFlower) && ingredients.Contains(IngredientType.CrystalFlower))
                return PotionType.FireCrystal;
        }

        return PotionType.FailedSludge;
    }
}