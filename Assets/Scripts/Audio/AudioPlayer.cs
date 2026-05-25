using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioPlayer : NetworkBehaviour
{
    [SerializeField] private List<SoundEffect> _sounds;

    private Dictionary<string, SoundEffect> _soundMap;
    private List<GameObject> _activeAudioObjects = new();

    private void Awake()
    {
        _soundMap = new Dictionary<string, SoundEffect>();

        foreach (SoundEffect sound in _sounds)
        {
            if (string.IsNullOrWhiteSpace(sound.Name))
            {
                Debug.LogWarning("AudioPlayer contains unnamed sound.");
                continue;
            }

            if (_soundMap.ContainsKey(sound.Name))
            {
                Debug.LogWarning($"Duplicate sound name: {sound.Name}");
                continue;
            }

            _soundMap.Add(sound.Name, sound);
        }
    }

    public void Play(string soundName)
    {
        PlayClientRpc(soundName);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayClientRpc(string soundName)
    {
        if (!_soundMap.TryGetValue(soundName, out SoundEffect sound))
        {
            Debug.LogWarning($"Sound not found: {soundName}");
            return;
        }

        if (sound == null || sound.Clip == null)
            return;

        CreateAndPlay(sound);
    }

    private void CreateAndPlay(SoundEffect sound)
    {
        GameObject audioObject = new GameObject($"Audio_{sound.Clip.name}");

        Follow follow = audioObject.AddComponent<Follow>();
        follow.Initialize(transform);

        AudioSource source = audioObject.AddComponent<AudioSource>();

        source.clip = sound.Clip;
        source.volume = sound.Volume;
        source.loop = sound.Loop;
        source.spatialBlend = sound.Spatial ? 1f : 0f;

        source.Play();
        _activeAudioObjects.Add(audioObject);

        if (!sound.Loop)
        {
            Destroy(audioObject, sound.Clip.length);
        }
    }

    public void Stop(string soundName)
    {
        StopClientRpc(soundName);
    }

    [Rpc(SendTo.Everyone)]
    private void StopClientRpc(string soundName)
    {
        if (!_soundMap.TryGetValue(soundName, out SoundEffect sound))
        {
            Debug.LogWarning($"Sound not found: {soundName}");
            return;
        }

        if (sound == null || sound.Clip == null)
            return;

        foreach (GameObject audioObject in new List<GameObject>(_activeAudioObjects))
        {
            if (audioObject == null)
            {
                _activeAudioObjects.Remove(audioObject);
                continue;    
            }

            AudioSource source = audioObject.GetComponent<AudioSource>();
            if (source != null && source.clip == sound.Clip)
            {
                source.Stop();
                Destroy(audioObject);
            }
        }
    }
}

[System.Serializable]
public class SoundEffect
{
    public string Name;
    public AudioClip Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;

    public bool Loop = false;
    public bool Spatial = true;
}