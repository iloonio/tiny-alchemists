using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Potion : MonoBehaviour
{
    public PotionType currentType;

    [Header("Crystal Platform")]
    [Tooltip("How long the crystal cube platform lasts (seconds)")]
    public float crystalPlatformDuration = 6f;
    [Tooltip("Size of the spawned crystal cube")]
    public float crystalPlatformSize = 2f;

    [Header("Fire-Crystal AOE")]
    [Tooltip("Radius of the AOE burn zone")]
    public float aoERadius = 4f;
    [Tooltip("Duration of the AOE burn zone")]
    public float aoeDuration = 5f;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    public void Initialize(PotionType type)
    {
        currentType = type;

        Renderer rend = GetComponent<Renderer>();
        if (type == PotionType.Fire)        rend.material.color = Color.red;
        else if (type == PotionType.Crystal) rend.material.color = Color.cyan;
        else if (type == PotionType.FireCrystal) rend.material.color = new Color(0.8f, 0.2f, 1f); // purple
        else if (type == PotionType.Explosive)   rend.material.color = new Color(1f, 0.5f, 0f);
        else if (type == PotionType.Sparkle)     rend.material.color = Color.yellow;
        else // FailedSludge or unhandled
            rend.material.color = new Color(0.4f, 0.3f, 0.2f);
    }

    public void OnPickedUp()
    {
        _rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 3f)
        {
            ExplodeEffect(collision);
            Destroy(gameObject);
        }
    }


    private void ExplodeEffect(Collision collision)
    {
        Debug.Log($"Potion {currentType} smashed on {collision.gameObject.name}!");

        GameObject hitObj = collision.gameObject;
        ContactPoint contact = collision.GetContact(0);
        Vector3 hitPoint = contact.point;

        switch (currentType)
        {
            case PotionType.Fire:
                HandleFire(hitObj);
                break;

            case PotionType.Crystal:
                HandleCrystal(hitObj, hitPoint);
                break;

            case PotionType.FireCrystal:
                HandleFireCrystal(hitObj, hitPoint);
                break;

            default:
                // FailedSludge, Sparkle, Explosive – no effect yet
                break;
        }
    }

    // ── Fire Potion ──
    private void HandleFire(GameObject hitObj)
    {
        // Try to apply fire to whatever we hit
        StatusEffectManager sem = hitObj.GetComponent<StatusEffectManager>();
        if (sem != null)
        {
            sem.ApplyFire();
            return;
        }

        // If the hit object is flammable
        FlammableObject flam = hitObj.GetComponent<FlammableObject>();
        if (flam != null)
        {
            flam.Ignite();
        }
    }

    // ── Crystal Potion ──
    private void HandleCrystal(GameObject hitObj, Vector3 hitPoint)
    {
        // If it hits a Player → crystallize them
        StatusEffectManager sem = hitObj.GetComponent<StatusEffectManager>();
        if (sem != null)
        {
            sem.ApplyCrystal();
            return;
        }

        // If it hits environment → spawn a temporary Crystal Cube platform
        SpawnCrystalPlatform(hitPoint);
    }

    // ── Fire + Crystal Potion ──
    private void HandleFireCrystal(GameObject hitObj, Vector3 hitPoint)
    {
        // 1) Always spawn the AOE burn zone at impact
        SpawnFireCrystalZone(hitPoint);

        // 2) Direct hit on a player → apply BOTH crystallized AND on-fire
        StatusEffectManager sem = hitObj.GetComponent<StatusEffectManager>();
        if (sem != null)
        {
            sem.ApplyCrystal();
            sem.ApplyFire();
        }
    }

    private void SpawnCrystalPlatform(Vector3 position)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "CrystalPlatform";
        cube.transform.position = position + Vector3.up * (crystalPlatformSize * 0.5f);
        cube.transform.localScale = Vector3.one * crystalPlatformSize;

        // Visual: semi-transparent cyan
        Renderer rend = cube.GetComponent<Renderer>();
        rend.material.color = new Color(0f, 1f, 1f, 0.6f);

        // Destroy after duration
        Destroy(cube, crystalPlatformDuration);

        Debug.Log($"<color=cyan>[Crystal]</color> Platform spawned at {position}, lasts {crystalPlatformDuration}s");
    }

    private void SpawnFireCrystalZone(Vector3 position)
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        zone.name = "FireCrystalZone";
        zone.transform.position = position;

        // Remove solid collider – the zone uses OverlapSphere internally
        Collider col = zone.GetComponent<Collider>();
        if (col != null) Destroy(col);

        FireCrystalZone fcz = zone.AddComponent<FireCrystalZone>();
        fcz.radius = aoERadius;
        fcz.duration = aoeDuration;

        Debug.Log($"<color=red>[FireCrystal]</color> AOE zone spawned at {position}");
    }
}
