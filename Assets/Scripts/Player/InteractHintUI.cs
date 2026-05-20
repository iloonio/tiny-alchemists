using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// InteractHintUI
/// =============================================================
/// Shows a pop-up hint near the crosshair when looking at a
/// holdable object.  All UI elements are created in Awake()
/// so no manual Canvas/Text setup is needed.
/// Attach to the Player prefab alongside PlayerInteract.
/// =============================================================
public class InteractHintUI : MonoBehaviour
{
    [SerializeField] private float _fontSize = 24f;
    [SerializeField] private Vector2 _offset = new(0f, -80f);

    private Canvas _canvas;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        // --- Canvas (screen-space overlay, owned by this player) ---

        // TODO: do not spawn new game objects in this manner!
        // Instead, add a canvas element to the player prefab
        // and update NetworkClient.cs to enable the canvas for owner only.
        GameObject canvasObj = new GameObject("HintCanvas");
        canvasObj.transform.SetParent(transform);

        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // --- Text (centered, slightly below crosshair) ---
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _text = textObj.AddComponent<TextMeshProUGUI>();
        _text.fontSize = _fontSize;
        _text.color = Color.white;
        _text.alignment = TextAlignmentOptions.Center;

        // TODO: This is deprecated. Use EnabledWordWrapping property instead.
        _text.enableWordWrapping = false;

        // outline for readability
        _text.outlineWidth = 0.2f;
        _text.outlineColor = Color.black;

        var rect = _text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = _offset;
        rect.sizeDelta = new Vector2(600f, 50f);

        _canvas.enabled = false;
    }

    public void Show(string message)
    {
        _text.text = message;
        _canvas.enabled = true;
    }

    public void Hide()
    {
        if (_canvas.enabled)
            _canvas.enabled = false;
    }
}
