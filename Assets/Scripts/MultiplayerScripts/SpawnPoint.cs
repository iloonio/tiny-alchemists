using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OawGizmos()
    {
      Gizmos.color = Color.cyan;
      Gizmos.DrawSphere(transform.position, 0.5f);
      Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
}
