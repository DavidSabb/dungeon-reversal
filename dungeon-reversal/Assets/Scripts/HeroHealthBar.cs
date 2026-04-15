using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HeroHealth))]
public class HeroHealthBar : MonoBehaviour
{
    [Header("Placement")]
    public Vector3 offset = new Vector3(0f, 2.4f, 0f);
    public Vector2 size   = new Vector2(1.2f, 0.15f);

    [Header("Colors")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);
    public Color fillColor       = new Color(0.85f, 0.15f, 0.15f, 1f);

    [Header("Behavior")]
    public bool hideWhenFull = false;

    private HeroHealth    _health;
    private Canvas        _canvas;
    private RectTransform _fillRT;
    private Camera        _cam;

    private void Awake()
    {
        _health = GetComponent<HeroHealth>();
        BuildBar();
    }

    private void Start() => _cam = Camera.main;

    private void BuildBar()
    {
        GameObject canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = offset;

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();

        RectTransform canvasRT = _canvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta  = new Vector2(100f, 100f * (size.y / size.x));
        canvasRT.localScale = Vector3.one * (size.x / 100f);

        GameObject bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        RawImage bg = bgGO.AddComponent<RawImage>();
        bg.color   = backgroundColor;
        bg.texture = Texture2D.whiteTexture;
        RectTransform bgRT = bg.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(canvasGO.transform, false);
        RawImage fill = fillGO.AddComponent<RawImage>();
        fill.color   = fillColor;
        fill.texture = Texture2D.whiteTexture;
        _fillRT = fill.rectTransform;
        _fillRT.anchorMin = Vector2.zero;
        _fillRT.anchorMax = Vector2.one;
        _fillRT.offsetMin = new Vector2(2f, 2f);
        _fillRT.offsetMax = new Vector2(-2f, -2f);
        _fillRT.pivot     = new Vector2(0f, 0.5f);
    }

    private void LateUpdate()
    {
        if (_fillRT == null || _health == null) return;

        float pct = _health.maxHealth > 0f
            ? Mathf.Clamp01(_health.currentHealth / _health.maxHealth)
            : 0f;

        _fillRT.localScale = new Vector3(pct, 1f, 1f);

        if (hideWhenFull && pct >= 0.999f)
            _canvas.enabled = false;
        else
            _canvas.enabled = true;

        if (_cam == null) _cam = Camera.main;
        if (_cam != null)
            _canvas.transform.rotation = _cam.transform.rotation;
    }
}
