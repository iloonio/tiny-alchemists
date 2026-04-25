using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

[RequireComponent(typeof(UIDocument))]
public class UIButtonLogger : MonoBehaviour
{
    [Tooltip("The name of the button in the UXML (Name field in UI Builder)")]
    [SerializeField] private string buttonName = "MyButton";
    [SerializeField] private NetworkedSceneManager _networkedSceneManager; 

    private UIDocument _uiDocument;
    private Button _button;

    private void OnEnable()
    {
        // 1. Get the UI Document component
        _uiDocument = GetComponent<UIDocument>();

        // 2. Query the rootVisualElement to find the button by name
        _button = _uiDocument.rootVisualElement.Q<Button>(buttonName);

        // If it's empty, try to find it in the scene automatically
        if (_networkedSceneManager == null)
        {
            _networkedSceneManager = FindFirstObjectByType<NetworkedSceneManager>();
        }

        // Now check if we actually found it
        if (_networkedSceneManager == null)
        {
            Debug.LogError($"{gameObject.name} is missing a NetworkedSceneManager in the scene!");
        }

        if (_button != null)
        {
            // 3. Register the callback function
            _button.clicked += OnButtonClicked;
            Debug.Log($"Successfully bound to button: {buttonName}");
        }
        else
        {
            Debug.LogError($"Could not find a Button named '{buttonName}' in the UI Document.");
        }
    }

    private void OnDisable()
    {
        // 4. Always unregister callbacks to prevent memory leaks or ghost clicks
        if (_button != null)
        {
            _button.clicked -= OnButtonClicked;
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log($"<color=cyan>UI Toolkit Event:</color> Button '{buttonName}' was clicked!");
        _networkedSceneManager.ChangeScene();
    }
}