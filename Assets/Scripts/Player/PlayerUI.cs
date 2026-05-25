using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _majorText;

    [Header("Hint Text Settings")]
    [SerializeField] private float _hintFontSize = 24f;
    [SerializeField] private Color _hintColor = Color.white;
    [SerializeField] private Color _hintOutlineColor = Color.black;
    [SerializeField] [Range(0f, 1f)] private float _hintOutlineWidth = 0.2f;

    public Canvas Canvas => _canvas;

    private void Awake()
    {
        if (_canvas != null)
        {
            _text.enabled = false;
            _majorText.enabled = false;
        }

        if (_text != null)
        {
            _text.fontSize = _hintFontSize;
            _text.color = _hintColor;
            _text.outlineColor = _hintOutlineColor;
            _text.outlineWidth = _hintOutlineWidth;
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