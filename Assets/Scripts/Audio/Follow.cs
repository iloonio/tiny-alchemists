
using UnityEngine;

public class Follow : MonoBehaviour
{
    private Transform _target;

    public void Initialize(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target != null) {
            transform.position = _target.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}