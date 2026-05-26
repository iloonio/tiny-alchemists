using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StatusEffectList", menuName = "Scriptable Objects/StatusEffectList")]
public class StatusEffectList : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public Status Status;
        public GameObject Prefab;
    }

    [SerializeField]
    private List<Entry> _entries = new List<Entry>();

    public IReadOnlyList<Entry> Entries => _entries;

    public GameObject GetPrefabForStatus(Status status)
    {
        if (status == null) return null;
        foreach (var e in _entries)
        {
            if (e.Status == status) return e.Prefab;
        }
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateMappings();
    }

    /// <summary>
    /// Checks that every Status ScriptableObject has a mapping in this list and logs a warning for missing mappings.
    /// This helps editors spot missing FX prefabs for statuses.
    /// </summary>
    public void ValidateMappings()
    {
        string[] guids = AssetDatabase.FindAssets("t:Status");
        var missing = new List<string>();

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var s = AssetDatabase.LoadAssetAtPath<Status>(path);
            if (s == null) continue;

            bool found = false;
            foreach (var e in _entries)
            {
                if (e.Status == s && e.Prefab != null)
                {
                    found = true;
                    break;
                }
            }

            if (!found) missing.Add(s.name);
        }

        if (missing.Count > 0)
        {
            Debug.LogWarning($"StatusEffectList '{name}' is missing prefab mappings for: {string.Join(", ", missing)}. Please assign prefabs in the inspector.");
        }
    }
#endif
}
