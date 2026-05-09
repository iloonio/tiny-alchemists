using UnityEngine;
using UnityEngine.UIElements; 

[RequireComponent(typeof(UIDocument))]
public class NetworkJoinUI : MonoBehaviour
{
    [SerializeField] private string buttonName = "StartButton";
    [SerializeField] private NetworkSceneManager _networkSceneManager; 

    private UIDocument _uiDocument;
    private Button _button;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _button = _uiDocument.rootVisualElement.Q<Button>(buttonName);

        if (_networkSceneManager == null)
        {
            Debug.LogError($"{gameObject.name} is missing a NetworkSceneManager in the scene!");
        }

        if (_button != null)
        {
            _button.clicked += OnButtonClicked;
        }
        else
        {
            Debug.LogError($"Could not find a Button named '{buttonName}' in the UI Document.");
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.clicked -= OnButtonClicked;
        }
    }

    private void OnButtonClicked()
    {
        _networkSceneManager.ChangeScene();
    }
}