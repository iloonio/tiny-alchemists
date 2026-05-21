using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees per second around each local axis (X,Y,Z)")]
    [SerializeField] private Vector3 rotationSpeed = new(0, 45f, 0);

    // public properties for the editor to see
    public Vector3 RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }


    private void Update()
    {
        // rotate every frame.
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RotatingPlatform))]
public class RotatingPlatformEditor : Editor
{
    // NOTICE: this was made with the help of Gemini
    private void OnSceneGUI()
    {
        RotatingPlatform platform = (RotatingPlatform)target;
        Transform t = platform.transform;
        Vector3 pos = t.position;
        Vector3 currentSpeed = platform.RotationSpeed;

        float baseOffset = 1.5f;
        float sensitivity = 0.02f;

        EditorGUI.BeginChangeCheck();

        // --- X AXIS (Red Handle & Ring) ---
        Handles.color = Color.red;
        if (Mathf.Abs(currentSpeed.x) > 0.1f)
        {
            Handles.DrawWireDisc(pos, t.right, baseOffset + Mathf.Abs(currentSpeed.x) * sensitivity);
        }
        Vector3 posX = pos + t.right * (baseOffset + currentSpeed.x * sensitivity);
        Handles.DrawLine(pos + t.right * baseOffset, posX, 1.5f);
        Vector3 newPosX = Handles.Slider(posX, t.right, HandleUtility.GetHandleSize(posX) * 0.15f, Handles.ConeHandleCap, 0.1f);
        float newSpeedX = (Vector3.Dot(newPosX - pos, t.right) - baseOffset) / sensitivity;
        Handles.Label(posX + t.up * 0.2f, $"X: {currentSpeed.x:F0}°/s");


        // --- Y AXIS (Green Handle & Ring) ---
        Handles.color = Color.green;
        if (Mathf.Abs(currentSpeed.y) > 0.1f)
        {
            Handles.DrawWireDisc(pos, t.up, baseOffset + Mathf.Abs(currentSpeed.y) * sensitivity);
        }
        Vector3 posY = pos + t.up * (baseOffset + currentSpeed.y * sensitivity);
        Handles.DrawLine(pos + t.up * baseOffset, posY, 1.5f);
        Vector3 newPosY = Handles.Slider(posY, t.up, HandleUtility.GetHandleSize(posY) * 0.15f, Handles.ConeHandleCap, 0.1f);
        float newSpeedY = (Vector3.Dot(newPosY - pos, t.up) - baseOffset) / sensitivity;
        Handles.Label(posY + t.right * 0.2f, $"Y: {currentSpeed.y:F0}°/s");


        // --- Z AXIS (Blue Handle & Ring) ---
        Handles.color = Color.blue;
        if (Mathf.Abs(currentSpeed.z) > 0.1f)
        {
            Handles.DrawWireDisc(pos, t.forward, baseOffset + Mathf.Abs(currentSpeed.z) * sensitivity);
        }
        Vector3 posZ = pos + t.forward * (baseOffset + currentSpeed.z * sensitivity);
        Handles.DrawLine(pos + t.forward * baseOffset, posZ, 1.5f);
        Vector3 newPosZ = Handles.Slider(posZ, t.forward, HandleUtility.GetHandleSize(posZ) * 0.15f, Handles.ConeHandleCap, 0.1f);
        float newSpeedZ = (Vector3.Dot(newPosZ - pos, t.forward) - baseOffset) / sensitivity;
        Handles.Label(posZ + t.up * 0.2f, $"Z: {currentSpeed.z:F0}°/s");


        // If the user dragged any of the 3D handles, record data and update
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(platform, "Modify Platform Rotation Speed");
            platform.RotationSpeed = new Vector3(newSpeedX, newSpeedY, newSpeedZ);
            EditorUtility.SetDirty(platform);
        }
    }
}
#endif
