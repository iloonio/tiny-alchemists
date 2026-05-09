using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlantPot : NetworkBehaviour
{

    [Header("Growth")]
    [SerializeField] private float growTime = 15f;

    [Header("Harvest")]
    [SerializeField] private Transform _harvestSpawnPoint;
    [SerializeField] private int _harvestSpawnCount = 2;
    [SerializeField] private float _harvestSpawnVerticalForce = 3f;
    [SerializeField] private float _harvestSpawnHorizontalForce = 5f;
    [SerializeField] private float _harvestSpawnInterval = 0.5f;
    [SerializeField] private float _cooldownAfterHarvest = 1f;

    private NetworkVariable<int> _plantedIngredientId = new();
    public bool IsGrowing => _plantedIngredientId.Value != 0;
    private bool _isPaused = false;
    private float _growProgress = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || IsGrowing) return;
        
        if (!other.TryGetComponent(out Ingredient ingredient)) return;

        _plantedIngredientId.Value = (int) ingredient.Type;
        ingredient.NetworkObject.Despawn();

        GrowServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GrowServerRpc()
    {
        StartCoroutine(GrowCoroutine());
    }
    
    private IEnumerator GrowCoroutine()
    {
        while (_growProgress < growTime)
        {
            yield return null;
            if (!_isPaused) _growProgress += Time.deltaTime;
        }

        yield return StartCoroutine(SpawnIngredients());

        yield return new WaitForSeconds(_cooldownAfterHarvest);

        _plantedIngredientId.Value = 0;
        _growProgress = 0f;
    }

    private IEnumerator SpawnIngredients()
    {
        IngredientType plantedIngredient = (IngredientType) _plantedIngredientId.Value;   

        for (int i = 0; i < _harvestSpawnCount; i++)
        {
            yield return new WaitForSeconds(_harvestSpawnInterval);

            GameObject ingredient = Instantiate(plantedIngredient.IngredientPrefab, _harvestSpawnPoint.position, Quaternion.identity);

            ingredient.GetComponent<NetworkObject>().Spawn(); 
            
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 horizontalDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 force = Vector3.up * _harvestSpawnVerticalForce + horizontalDirection * _harvestSpawnHorizontalForce;
            ingredient.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        }
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void UnPause()
    {
        _isPaused = false;
    }
    
}
