using System.Collections;
using UnityEngine;

/// Attach to any environment prop that can catch fire and be destroyed.
public class FlammableObject : MonoBehaviour
{
    [Tooltip("Time in seconds before this object is destroyed after igniting")]
    public float burnDuration = 3f;

    [Tooltip("Radius to spread fire to nearby Flammable objects")]
    public float spreadRadius = 2f;

    [Tooltip("Delay before fire spreads to neighbours")]
    public float spreadDelay = 0.5f;

    private bool _isBurning;

    public void Ignite()
    {
        if (_isBurning) return;
        _isBurning = true;
        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        Debug.Log($"<color=orange>[Fire]</color> {gameObject.name} ignited!");

        // Visual feedback – tint orange
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(1f, 0.4f, 0f);

        // Wait a moment then spread
        yield return new WaitForSeconds(spreadDelay);
        SpreadFire();

        // Burn for remaining duration then destroy
        yield return new WaitForSeconds(burnDuration - spreadDelay);
        Debug.Log($"<color=orange>[Fire]</color> {gameObject.name} destroyed by fire.");
        Destroy(gameObject);
    }

    private void SpreadFire()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, spreadRadius);
        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Flammable")) continue;

            FlammableObject other = col.GetComponent<FlammableObject>();
            if (other != null) other.Ignite();
        }
    }
}
