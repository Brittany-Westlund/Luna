using System.Collections;
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

    [Header("Auto Detection")]
    [SerializeField] private bool useAutoDetection = true;
    [SerializeField] private string teapotTag = "Teapot";
    [SerializeField] private string teacupTag = "Teacup";
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private float detectionInterval = 0.1f;

    [Header("Auto Timing")]
    [Tooltip("Delay before the candle lights after teapot appears.")]
    public float lightDelay = 0f;

    [Tooltip("Delay before the candle extinguishes after teapot disappears / brewing happens.")]
    public float extinguishDelay = 0f;

    [Tooltip("Delay before the candle fully hides after tea is finished.")]
    public float hideDelay = 0f;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool candleWasLitForThisTeapot = false;
    private bool candleIsExtinguished = false;

    private Coroutine lightRoutine;
    private Coroutine extinguishRoutine;
    private Coroutine hideRoutine;
    private Coroutine autoRoutine;

    private bool lastDetectedTeapot = false;
    private bool lastDetectedTeacup = false;

    private enum CandleVisualState
    {
        Hidden,
        Lit,
        Unlit
    }

    private CandleVisualState currentState = CandleVisualState.Hidden;

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

    private void OnEnable()
    {
        if (useAutoDetection)
        {
            autoRoutine = StartCoroutine(AutoDetectRoutine());
        }
    }

    private void OnDisable()
    {
        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        StopAllCandleRoutines();
    }

    public void NotifyTeapotSpawned()
    {
        StopHideRoutine();

        if (currentState == CandleVisualState.Lit)
            return;

        StopLightRoutine();
        lightRoutine = StartCoroutine(LightAfterDelay());

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeapotSpawned on '{name}'. Scheduling candle light after {lightDelay}s.");
    }

    public void NotifyTeapotGoneAfterBrewing()
    {
        if (!candleWasLitForThisTeapot && currentState != CandleVisualState.Lit)
            return;

        StopExtinguishRoutine();
        extinguishRoutine = StartCoroutine(ExtinguishAfterDelay());

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeapotGoneAfterBrewing on '{name}'. Scheduling extinguish after {extinguishDelay}s.");
    }

    public void NotifyTeaFinished()
    {
        StopHideRoutine();
        hideRoutine = StartCoroutine(HideAfterDelay());

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] NotifyTeaFinished on '{name}'. Scheduling hide/reset after {hideDelay}s.");
    }

    private IEnumerator AutoDetectRoutine()
    {
        while (true)
        {
            bool teapotNearby = IsTaggedObjectNearby(teapotTag);
            bool teacupNearby = IsTaggedObjectNearby(teacupTag);

            if (debugLogs && (teapotNearby != lastDetectedTeapot || teacupNearby != lastDetectedTeacup))
            {
                Debug.Log($"[LilystoolCandleController] AutoDetect '{name}' -> teapotNearby={teapotNearby}, teacupNearby={teacupNearby}, state={currentState}");
            }

            // Teapot appeared -> light candle
            if (teapotNearby && !lastDetectedTeapot)
            {
                NotifyTeapotSpawned();
            }

            // Teapot disappeared after being present -> extinguish
            if (!teapotNearby && lastDetectedTeapot && currentState == CandleVisualState.Lit)
            {
                NotifyTeapotGoneAfterBrewing();
            }

            // Teacup appeared and candle is already unlit -> hide/reset after delay
            if (teacupNearby && !lastDetectedTeacup && currentState == CandleVisualState.Unlit)
            {
                NotifyTeaFinished();
            }

            // If everything is gone and we are still unlit, finish hiding
            if (!teapotNearby && !teacupNearby && currentState == CandleVisualState.Unlit)
            {
                NotifyTeaFinished();
            }

            lastDetectedTeapot = teapotNearby;
            lastDetectedTeacup = teacupNearby;

            yield return new WaitForSeconds(detectionInterval);
        }
    }

    private bool IsTaggedObjectNearby(string tagName)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tagName);
        float maxDistSq = detectionRadius * detectionRadius;
        Vector2 origin = transform.position;

        for (int i = 0; i < objs.Length; i++)
        {
            GameObject obj = objs[i];
            if (obj == null)
                continue;

            float distSq = ((Vector2)obj.transform.position - origin).sqrMagnitude;
            if (distSq <= maxDistSq)
                return true;
        }

        return false;
    }

    private IEnumerator LightAfterDelay()
    {
        if (lightDelay > 0f)
            yield return new WaitForSeconds(lightDelay);

        ShowLitCandle();
        candleWasLitForThisTeapot = true;
        candleIsExtinguished = false;
        currentState = CandleVisualState.Lit;
        lightRoutine = null;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] Candle lit on '{name}'.");
    }

    private IEnumerator ExtinguishAfterDelay()
    {
        if (extinguishDelay > 0f)
            yield return new WaitForSeconds(extinguishDelay);

        ExtinguishCandle();
        candleIsExtinguished = true;
        currentState = CandleVisualState.Unlit;
        extinguishRoutine = null;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] Candle extinguished on '{name}'.");
    }

    private IEnumerator HideAfterDelay()
    {
        if (hideDelay > 0f)
            yield return new WaitForSeconds(hideDelay);

        ResetCandleToHidden();
        currentState = CandleVisualState.Hidden;
        hideRoutine = null;

        if (debugLogs)
            Debug.Log($"[LilystoolCandleController] Candle hidden/reset on '{name}'.");
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

    private void StopAllCandleRoutines()
    {
        StopLightRoutine();
        StopExtinguishRoutine();
        StopHideRoutine();
    }

    private void StopLightRoutine()
    {
        if (lightRoutine != null)
        {
            StopCoroutine(lightRoutine);
            lightRoutine = null;
        }
    }

    private void StopExtinguishRoutine()
    {
        if (extinguishRoutine != null)
        {
            StopCoroutine(extinguishRoutine);
            extinguishRoutine = null;
        }
    }

    private void StopHideRoutine()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }
}