using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParticlePlayer : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _particles;

    private Dictionary<string, GameObject> _particleMap;
    private List<GameObject> _activeParticleObjects = new();

    private void Awake()
    {
        _particleMap = new Dictionary<string, GameObject>();

        foreach (GameObject particle in _particles)
        {
            if (particle == null)
            {
                Debug.LogError($"Invalid particle: {particle}");
                continue;
            }

            _particleMap.Add(particle.name, particle);
        }
    }

    public void Play(string particleName)
    {
        PlayClientRpc(particleName);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayClientRpc(string particleName)
    {
        if (!_particleMap.TryGetValue(particleName, out GameObject particle))
        {
            Debug.LogWarning($"Particle not found: {particleName}");
            return;
        }

        if (particle == null)
            return;

        CreateAndPlay(particle);
    }

    private void CreateAndPlay(GameObject particle)
    {
        GameObject particleObject = Instantiate(particle, transform.position, particle.transform.rotation);

        Follow follow = particleObject.AddComponent<Follow>();
        follow.Initialize(transform);

        _activeParticleObjects.Add(particleObject);

        if (particleObject.TryGetComponent(out ParticleSystem particleSystem))
        {
            if (!particleSystem.main.loop)
            {
                Destroy(particleObject, particleSystem.main.duration);
            }
        }
    }

    public void Stop(string particleName)
    {
        StopClientRpc(particleName);
    }

    [Rpc(SendTo.Everyone)]
    private void StopClientRpc(string particleName)
    {
        if (!_particleMap.TryGetValue(particleName, out GameObject particle))
        {
            Debug.LogWarning($"Particle not found: {particleName}");
            return;
        }

        if (particle == null)
            return;

        foreach (GameObject particleObject in new List<GameObject>(_activeParticleObjects))
        {
            if (particleObject == null)
            {
                _activeParticleObjects.Remove(particleObject);
                continue;    
            }

            if (particleObject.name.StartsWith(particle.name))
            {
                _activeParticleObjects.Remove(particleObject);
                Destroy(particleObject);
            }
        }
    }
}