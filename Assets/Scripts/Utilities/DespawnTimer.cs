using System;
using UnityEngine;

public class DespawnTimer : MonoBehaviour
{
    [SerializeField] private float timeUntilDestroyed = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, timeUntilDestroyed);
    }
}
