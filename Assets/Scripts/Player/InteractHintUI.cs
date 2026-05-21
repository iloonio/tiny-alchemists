using TMPro;
using UnityEngine;

/// InteractHintUI
/// =============================================================
/// Shows a pop-up hint near the crosshair when looking at a
/// holdable object.
/// =============================================================
public class InteractHintUI : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _text;

    public Canvas Canvas => _canvas;

    private void Awake()
    {
        if (_canvas != null)
            _text.enabled = false;
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
}
