using UnityEngine;
using UnityEngine.UI;

public class UIPulsateFade : MonoBehaviour
{
    [Header("Scale Pulse")]
    [SerializeField] private float minScale = 0.95f;
    [SerializeField] private float maxScale = 1.05f;
    [SerializeField] private float scaleSpeed = 2f;

    [Header("Alpha Pulse")]
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float alphaSpeed = 2f;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private RectTransform rectTransform;
    private Image image;

    private float scaleTimer;
    private float alphaTimer;

    private float currentAlphaMultiplier = 0f;
    private bool fadingIn = false;
    private bool fadingOut = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void OnEnable()
    {
        scaleTimer = Random.Range(0f, 100f);
        alphaTimer = Random.Range(0f, 100f);

        currentAlphaMultiplier = 0f;
        fadingIn = true;
        fadingOut = false;
    }

    void OnDisable()
    {
        fadingIn = false;
        fadingOut = false;
    }

    void Update()
    {
        if (rectTransform == null || image == null)
            return;

        float dt = Time.deltaTime;

        // --- Fade In ---
        if (fadingIn)
        {
            if (fadeInDuration <= 0f)
            {
                currentAlphaMultiplier = 1f;
                fadingIn = false;
            }
            else
            {
                currentAlphaMultiplier += dt / fadeInDuration;
                if (currentAlphaMultiplier >= 1f)
                {
                    currentAlphaMultiplier = 1f;
                    fadingIn = false;
                }
            }
        }

        // --- Scale Pulse ---
        scaleTimer += dt * scaleSpeed;
        float scaleT = (Mathf.Sin(scaleTimer) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, scaleT);
        rectTransform.localScale = new Vector3(scale, scale, 1f);

        // --- Alpha Pulse ---
        alphaTimer += dt * alphaSpeed;
        float alphaT = (Mathf.Sin(alphaTimer) + 1f) * 0.5f;
        float baseAlpha = Mathf.Lerp(minAlpha, maxAlpha, alphaT);

        float finalAlpha = baseAlpha * currentAlphaMultiplier;

        Color c = image.color;
        c.a = finalAlpha;
        image.color = c;
    }

    // Optional: call this before disabling if you want a fade-out instead of instant off
    public void FadeOutAndDisable()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(FadeOutRoutine());
    }

    private System.Collections.IEnumerator FadeOutRoutine()
    {
        fadingIn = false;
        fadingOut = true;

        float t = 0f;
        float start = currentAlphaMultiplier;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeOutDuration);
            currentAlphaMultiplier = Mathf.Lerp(start, 0f, lerp);
            yield return null;
        }

        currentAlphaMultiplier = 0f;
        gameObject.SetActive(false);
    }
}