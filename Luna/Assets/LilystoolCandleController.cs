using UnityEngine;

public class LilystoolCandleController : MonoBehaviour
{
    [Header("Candle References")]
    public GameObject candleRoot;
    public SpriteRenderer candleSpriteRenderer;
    public Animator candleAnimator;

    [Header("Unlit State")]
    [Tooltip("Explicit sprite to force after extinguishing.")]
    public Sprite unlitSprite;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool candleWasLitForThisTeapot = false;
    private bool candleIsExtinguished = false;

    private void Awake()
    {
        if (candleRoot == null)
        {
            Transform candle = transform.Find("MoonlightCandle");
            if (candle != null)
                candleRoot = candle.gameObject;
        }

        if (candleRoot != null && candleSpriteRenderer == null)
            candleSpriteRenderer = candleRoot.GetComponent<SpriteRenderer>();

        if (candleRoot != null && candleAnimator == null)
            candleAnimator = candleRoot.GetComponent<Animator>();

        ResetCandleToHidden();

        if (debugLogs)
        {
            Debug.Log($"[LilystoolCandleController] Awake on '{name}'. candleRoot={(candleRoot != null ? candleRoot.name : "NULL")}, candleAnimator={(candleAnimator != null ? candleAnimator.name : "NULL")}");
        }
    }

    public void NotifyTeapotSpawned()
    {
        ShowLitCandle();
        candleWasLitForThisTeapot = true;
        candleIsExtinguished = false;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeapotSpawned on '{name}'. Candle lit.");
    }

    public void NotifyTeapotGoneAfterBrewing()
    {
        if (!candleWasLitForThisTeapot)
            return;

        ExtinguishCandle();
        candleIsExtinguished = true;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeapotGoneAfterBrewing on '{name}'. Candle extinguished.");
    }

    public void NotifyTeaFinished()
    {
        ResetCandleToHidden();

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeaFinished on '{name}'. Candle hidden/reset.");
    }

    private void ShowLitCandle()
    {
        if (candleRoot == null)
            return;

        if (!candleRoot.activeSelf)
            candleRoot.SetActive(true);

        if (candleSpriteRenderer != null)
            candleSpriteRenderer.enabled = true;

        if (candleAnimator != null)
        {
            candleAnimator.enabled = true;
            candleAnimator.Play(0, 0, 0f);
            candleAnimator.Update(0f);
        }
    }

    private void ExtinguishCandle()
    {
        if (candleRoot == null)
            return;

        if (!candleRoot.activeSelf)
            candleRoot.SetActive(true);

        if (candleSpriteRenderer != null)
            candleSpriteRenderer.enabled = true;

        if (candleAnimator != null)
            candleAnimator.enabled = false;

        if (candleSpriteRenderer != null && unlitSprite != null)
            candleSpriteRenderer.sprite = unlitSprite;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] Forced unlit sprite on '{name}'.");
    }

    private void ResetCandleToHidden()
    {
        candleWasLitForThisTeapot = false;
        candleIsExtinguished = false;

        if (candleAnimator != null)
            candleAnimator.enabled = false;

        if (candleSpriteRenderer != null && unlitSprite != null)
            candleSpriteRenderer.sprite = unlitSprite;

        if (candleRoot != null)
            candleRoot.SetActive(false);
    }
}