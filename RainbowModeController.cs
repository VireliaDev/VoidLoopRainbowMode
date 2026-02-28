using UnityEngine;
using UnityEngine.UI;

public class RainbowModeController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Image image;
    private float hue;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        image = gameObject.AddComponent<Image>();
        image.raycastTarget = false;

        var rect = transform as RectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnEnable()
    {
        SettingsManager.OnSettingsChanged += Apply;
        Apply();
    }

    private void OnDisable()
    {
        SettingsManager.OnSettingsChanged -= Apply;
    }

    private void Update()
    {
        if (canvasGroup.alpha <= 0f) return;

        hue += Time.deltaTime * 0.4f;
        if (hue > 1f) hue = 0f;

        image.color = Color.HSVToRGB(hue, 1f, 1f);
    }

    private void Apply()
    {
        var enabled = PlayerPrefs.GetInt("rainbowMode", 0) == 1;
        canvasGroup.alpha = enabled ? 0.35f : 0f;
    }
}