using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(WaveManager))]
public class TutorialDirector : MonoBehaviour
{
    public float bannerDuration = 2.5f;
    public Color wave1Color = new Color(0.6f, 1f, 0.6f);
    public Color wave2Color = new Color(1f, 0.85f, 0.4f);
    public Color wave3Color = new Color(1f, 0.4f, 0.4f);

    WaveManager wm;
    Canvas bannerCanvas;
    CanvasGroup bannerGroup;
    TextMeshProUGUI bannerTitle;
    TextMeshProUGUI bannerSubtitle;

    void Start()
    {
        wm = GetComponent<WaveManager>();
        BuildBanner();
        wm.OnWaveStart += OnWaveStart;
        wm.OnWaveEnd += OnWaveEnd;
        wm.OnAllWavesComplete += OnAllClear;
    }

    void OnDestroy()
    {
        if (wm == null) return;
        wm.OnWaveStart -= OnWaveStart;
        wm.OnWaveEnd -= OnWaveEnd;
        wm.OnAllWavesComplete -= OnAllClear;
    }

    void OnWaveStart(int wave)
    {
        Color c = wave == 1 ? wave1Color : wave == 2 ? wave2Color : wave3Color;
        string sub = "";
        if (wave == 1) sub = "The scouts arrive";
        else if (wave == 2) sub = "Reinforcements - they hit harder";
        else if (wave == 3) sub = "The Champions - survive them all";
        StartCoroutine(ShowBanner("WAVE " + wave, sub, c));
    }

    void OnWaveEnd(int wave)
    {
        if (wave < wm.totalWaves)
            StartCoroutine(ShowBanner("WAVE CLEAR", "Next wave incoming", Color.white));
    }

    void OnAllClear()
    {
        StartCoroutine(ShowBanner("ALL WAVES CLEARED", "Victory!", new Color(0.6f, 1f, 0.8f)));
    }

    void BuildBanner()
    {
        GameObject canvasGO = new GameObject("BannerCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasGO.transform.SetParent(transform, false);
        bannerCanvas = canvasGO.GetComponent<Canvas>();
        bannerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        bannerCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        bannerGroup = canvasGO.GetComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;
        bannerGroup.interactable = false;

        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.SetParent(bannerCanvas.transform, false);
        titleRT.anchorMin = new Vector2(0.5f, 0.65f);
        titleRT.anchorMax = new Vector2(0.5f, 0.65f);
        titleRT.sizeDelta = new Vector2(1200f, 120f);
        bannerTitle = titleGO.GetComponent<TextMeshProUGUI>();
        bannerTitle.alignment = TextAlignmentOptions.Center;
        bannerTitle.fontSize = 96;
        bannerTitle.fontStyle = FontStyles.Bold;

        GameObject subGO = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform subRT = subGO.GetComponent<RectTransform>();
        subRT.SetParent(bannerCanvas.transform, false);
        subRT.anchorMin = new Vector2(0.5f, 0.55f);
        subRT.anchorMax = new Vector2(0.5f, 0.55f);
        subRT.sizeDelta = new Vector2(1200f, 60f);
        bannerSubtitle = subGO.GetComponent<TextMeshProUGUI>();
        bannerSubtitle.alignment = TextAlignmentOptions.Center;
        bannerSubtitle.fontSize = 36;
        bannerSubtitle.color = Color.white;
    }

    IEnumerator ShowBanner(string title, string subtitle, Color color)
    {
        if (bannerTitle == null) yield break;
        bannerTitle.text = title;
        bannerTitle.color = color;
        bannerSubtitle.text = subtitle;

        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            bannerGroup.alpha = t / 0.35f;
            yield return null;
        }
        bannerGroup.alpha = 1f;
        yield return new WaitForSeconds(bannerDuration);
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            bannerGroup.alpha = 1f - (t / 0.5f);
            yield return null;
        }
        bannerGroup.alpha = 0f;
    }
}
