using System.Collections.Generic;
using UnityEngine;


//  MagneticRegistry.cs — Singleton that tracks magnetic entities
//
//  Any delivery zone/cube with Magnetic registers its targets.
//  Every FixedUpdate, all registered entities pull toward each other.
//
//  Special Interactions handled here:
//    Size+Magnetic  → increased mass → stronger pull (automatic)
//    Bouncy+Magnetic → REPEL instead of attract
//


public class MagneticRegistry : MonoBehaviour
{
    private static MagneticRegistry _instance;
    public static MagneticRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[MagneticRegistry]");
                _instance = go.AddComponent<MagneticRegistry>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public struct MagneticEntry
    {
        public Rigidbody rb;
        public float expireTime;
        public bool repels; // Bouncy+Magnetic special interaction
    }

    private List<MagneticEntry> _entries = new List<MagneticEntry>();

    [Header("Tuning")]
    public float baseStrength = 10f;


    // Register a Rigidbody as magnetic for a duration.
    /// <param name="rb">The Rigidbody to magnetize.</param>
    /// <param name="duration">-1 = permanent (until manually removed).</param>
    /// <param name="repels">True if Bouncy+Magnetic interaction → repels instead.</param>
    public void Register(Rigidbody rb, float duration = -1f, bool repels = false)
    {
        if (rb == null) return;

        // Don't double-register
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].rb == rb)
            {
                // Update expiry and repel flag
                var e = _entries[i];
                e.expireTime = duration > 0 ? Time.time + duration : float.MaxValue;
                e.repels = repels;
                _entries[i] = e;
                return;
            }
        }

        _entries.Add(new MagneticEntry
        {
            rb = rb,
            expireTime = duration > 0 ? Time.time + duration : float.MaxValue,
            repels = repels
        });
    }

    public void Unregister(Rigidbody rb)
    {
        _entries.RemoveAll(e => e.rb == rb);
    }

    void FixedUpdate()
    {
        // Clean up expired / destroyed entries
        _entries.RemoveAll(e => e.rb == null || Time.time > e.expireTime);

        // Mutual attraction (or repulsion) between all pairs
        for (int i = 0; i < _entries.Count; i++)
        {
            for (int j = i + 1; j < _entries.Count; j++)
            {
                var a = _entries[i];
                var b = _entries[j];

                if (a.rb.isKinematic && b.rb.isKinematic) continue;

                Vector3 delta = b.rb.position - a.rb.position;
                float dist = delta.magnitude;
                if (dist < 0.3f) continue;

                // Force = strength * massA * massB / dist^2 (simplified gravity-like)
                float forceMag = baseStrength * a.rb.mass * b.rb.mass / Mathf.Max(dist * dist, 1f);
                Vector3 dir = delta.normalized;

                // Bouncy+Magnetic special: either one repels → both repel
                bool repel = a.repels || b.repels;
                if (repel) dir = -dir;

                if (!a.rb.isKinematic)
                    a.rb.AddForce(dir * forceMag, ForceMode.Force);
                if (!b.rb.isKinematic)
                    b.rb.AddForce(-dir * forceMag, ForceMode.Force);
            }
        }
    }
}
