using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Potion : MonoBehaviour
{
    public PotionType currentType;
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
        if (type == PotionType.Fire) rend.material.color = Color.red;
        else if (type == PotionType.Crystal) rend.material.color = Color.cyan;
        else if (type == PotionType.Explosive) rend.material.color = new Color(1f, 0.5f, 0f); // 
        else if (type == PotionType.Sparkle) rend.material.color = Color.yellow;
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
    }
}