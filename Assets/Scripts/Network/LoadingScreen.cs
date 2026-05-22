using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// LoadingScreen
/// =============================================================
/// Renders a full-screen black overlay with fade-in / fade-out.
/// Attach to the same GameObject as NetworkSceneManager (which is
/// DontDestroyOnLoad) so the overlay survives scene transitions.
///
/// The Canvas, Image, and CanvasGroup are built automatically in
/// Awake() — no manual UI setup required.
/// =============================================================
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;
    private Canvas _canvas;

    private void Awake()
    {
        // --- build overlay UI in code so there's nothing to set up manually ---
        // child GameObject to hold all UI components
        GameObject overlay = new GameObject("LoadingOverlay");
        overlay.transform.SetParent(transform);

        // Canvas — renders on top of everything
        _canvas = overlay.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;

        // CanvasScaler for consistent sizing
        var scaler = overlay.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        overlay.AddComponent<GraphicRaycaster>();

        // full-screen black image
        GameObject imageObj = new GameObject("BlackScreen");
        imageObj.transform.SetParent(overlay.transform, false);

        var image = imageObj.AddComponent<Image>();
        image.color = Color.black;

        // stretch to fill entire screen
        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // CanvasGroup on the overlay for alpha control
        _canvasGroup = overlay.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvas.enabled = false;
    }

    // Fade to black, execute an action, then fade back in.
    // Usage:  StartCoroutine(loadingScreen.Transition(() => { ... }));
    public IEnumerator Transition(Action onScreenBlack)
    {
        // fade to black
        yield return Fade(0f, 1f);

        // screen is now fully black — do the heavy work (e.g. load scene)
        onScreenBlack?.Invoke();
    }


    // Fade from black back to transparent.  Call after the new scene is ready.
    public IEnumerator FadeOut()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        _canvas.enabled = true;
        _canvasGroup.alpha = from;

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = to;

        // hide canvas entirely when transparent so it doesn't block input
        if (Mathf.Approximately(to, 0f))
            _canvas.enabled = false;
    }
}
