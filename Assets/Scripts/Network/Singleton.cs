using System;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _type;

    private void Awake()
    {
        if (FindObjectsByType(_type.GetType(), FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}