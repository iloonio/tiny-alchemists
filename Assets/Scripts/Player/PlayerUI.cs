using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _majorText;

    public Canvas Canvas => _canvas;

    private void Awake()
    {
        if (_canvas != null) {
            _text.enabled = false;
            _majorText.enabled = false;
        }
    }

    public void Show(string message)
    {
        _text.text = message;
        _text.enabled = true;
    }

    public void Hide()
    {
        if (_canvas != null && _text.enabled)
            _text.enabled = false;
    }

    public void ShowMajor(string message)
    {
        Hide();
        _majorText.text = message;
        _majorText.enabled = true;
    }

    public void HideMajor()
    {
        if (_canvas != null && _majorText.enabled)
            _majorText.enabled = false;
    }
}
