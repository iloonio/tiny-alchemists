using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AudioPlayer : NetworkBehaviour
{
    [SerializeField] private List<SoundEffect> _sounds;

    private Dictionary<string, SoundEffect> _soundMap;

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
        source.spatialBlend = 1f;

        source.Play();

        if (!sound.Loop)
        {
            Destroy(audioObject, sound.Clip.length);
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
}