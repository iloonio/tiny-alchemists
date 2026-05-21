using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// MovingPlatform handles movement of platforms, and it communicates with PlayerMove.cs
/// In order to ensure that the player is affected by the velocity of the platform when they
/// are standing on it.
///
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("MovementSettings")]
    [SerializeField] private Vector3 localPointA = new(-3, 0, 0);
    [SerializeField] private Vector3 localPointB = new(3, 0, 0);
    [SerializeField] private float speed = 3f;
    [Tooltip("whether the speed of the platform should change linearly or remain constant as it moves. Set to true to have it change linearly.")]
    [SerializeField] private bool useEasing = true;

    // public properties for the editor to see
    public Vector3 LocalPointA { get => localPointA; set => localPointA = value; }
    public Vector3 LocalPointB { get => localPointB; set => localPointB = value; }
    public bool UseEasing { get => useEasing; set => useEasing = value; }
    public Vector3 Velocity { get; private set; }

    private Rigidbody _rb;
    private Vector3 worldPointA;
    private Vector3 worldPointB;
    private float progress = 0f; //0 = at A, 1 = at B
    private int direction = 1; // are we moving towards A, or towards B?
    private Vector3 _lastPosition; // tracks position of platform from the previous fixed update.
    private Vector3 _targetPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        worldPointA = transform.TransformPoint(localPointA);
        worldPointB = transform.TransformPoint(localPointB);

        // start at Point A, move towards point B.
        _targetPosition = worldPointA;
        _rb.position = worldPointA;
        _lastPosition = _rb.position;
    }

    private void Update()
    {
        float step = (speed * Time.deltaTime) / Vector3.Distance(worldPointA, worldPointB);
        progress += step * direction;

        //alternate between 0 and 1
        if (progress >= 1f || progress <= 0f) direction *= -1; //inverse direction when we reach A or B
        progress = Mathf.Clamp01(progress); //Clamp01 specifically clamps between 0 and 1.

        float t = useEasing ? Mathf.SmoothStep(0f, 1f, progress) : progress;
        _targetPosition = Vector3.Lerp(worldPointA, worldPointB, t);
    }

    // Move the kinematic rigidbody in FixedUpdate so physics samples a smooth velocity.
    private void FixedUpdate()
    {
        _rb.MovePosition(_targetPosition);
        Velocity = (_rb.position - _lastPosition) / Time.fixedDeltaTime;
        _lastPosition = _rb.position;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
    // NOTICE: this was made with the help of Gemini

    public override void OnInspectorGUI()
    {
        //not sure what this accomplishes. but ummm thanks gemini?
        base.OnInspectorGUI();
    }

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
