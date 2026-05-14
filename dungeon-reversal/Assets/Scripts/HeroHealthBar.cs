using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HeroHealth))]
public class HeroHealthBar : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 2.4f, 0f);
    public Vector2 size = new Vector2(1.2f, 0.15f);
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.65f);
    public Color fillColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    public bool hideWhenFull = false;

    HeroHealth health;
    Canvas canvas;
    RectTransform fillRT;
    Camera cam;

    void Awake()
    {
        health = GetComponent<HeroHealth>();
        BuildBar();
    }

    void Start()
    {
        cam = Camera.main;
    }

    void BuildBar()
    {
        GameObject canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = offset;

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(100f, 100f * (size.y / size.x));
        canvasRT.localScale = Vector3.one * (size.x / 100f);

        GameObject bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        RawImage bg = bgGO.AddComponent<RawImage>();
        bg.color = backgroundColor;
        bg.texture = Texture2D.whiteTexture;
        RectTransform bgRT = bg.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(canvasGO.transform, false);
        RawImage fill = fillGO.AddComponent<RawImage>();
        fill.color = fillColor;
        fill.texture = Texture2D.whiteTexture;
        fillRT = fill.rectTransform;
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(2f, 2f);
        fillRT.offsetMax = new Vector2(-2f, -2f);
        fillRT.pivot = new Vector2(0f, 0.5f);
    }

    void LateUpdate()
    {
        if (fillRT == null || health == null) return;

        float pct = health.maxHealth > 0f ? Mathf.Clamp01(health.currentHealth / health.maxHealth) : 0f;
        fillRT.localScale = new Vector3(pct, 1f, 1f);

        canvas.enabled = !(hideWhenFull && pct >= 0.999f);

        if (cam == null) cam = Camera.main;
        if (cam != null) canvas.transform.rotation = cam.transform.rotation;
    }
}
