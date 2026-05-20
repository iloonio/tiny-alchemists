using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovingPlatform : MonoBehaviour
{
    [Header("MovementSettings")]
    [SerializeField] private Vector3 localPointA = new(-3, 0, 0);
    [SerializeField] private Vector3 localPointB = new(3, 0, 0);
    [SerializeField] private float speed = 3f;

    // public properties for the editor to see
    public Vector3 LocalPointA { get => localPointA; set => localPointA = value; }
    public Vector3 LocalPointB { get => localPointB; set => localPointB = value; }

    private Vector3 worldPointA;
    private Vector3 worldPointB;
    private Vector3 nextTarget;

    private void Start()
    {
        worldPointA = transform.TransformPoint(localPointA);
        worldPointB = transform.TransformPoint(localPointB);

        // start at Point A, move towards point B.
        transform.position = worldPointA;
        nextTarget = worldPointB;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextTarget, speed * Time.deltaTime);

        // switch targets if we are close enough
        if (Vector3.Distance(transform.position, nextTarget) < 0.001f)
        {
            nextTarget = (nextTarget == worldPointA) ? worldPointB : worldPointA;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    // NOTICE: this was made with the help of Gemini
    private void OnSceneGUI()
    {
        MovingPlatform platform = (MovingPlatform)target;
        Transform transform = platform.transform;

        // 1. Convert local points to World Space so the Handles draw in the right place
        Vector3 worldA = transform.TransformPoint(platform.LocalPointA);
        Vector3 worldB = transform.TransformPoint(platform.LocalPointB);

        // 2. Draw a visual line connecting the two points
        Handles.color = Color.cyan;
        Handles.DrawLine(worldA, worldB, 2.5f);

        // 3. Look for changes in the Scene view handles
        EditorGUI.BeginChangeCheck();

        // Draw standard position handles (arrows) for both points
        Vector3 newWorldA = Handles.PositionHandle(worldA, Quaternion.identity);
        Vector3 newWorldB = Handles.PositionHandle(worldB, Quaternion.identity);

        // 4. If the user dragged a handle, save the new position
        if (EditorGUI.EndChangeCheck())
        {
            // Allow Ctrl+Z / Undo functionality in Unity
            Undo.RecordObject(platform, "Move Platform Endpoints");

            // Convert back to Local Space before saving
            platform.LocalPointA = transform.InverseTransformPoint(newWorldA);
            platform.LocalPointB = transform.InverseTransformPoint(newWorldB);

            // Force the inspector to redraw if values changed
            EditorUtility.SetDirty(platform);
        }
    }
}
#endif
