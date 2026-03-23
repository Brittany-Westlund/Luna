using UnityEngine;

public class CandleController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional explicit teapot root. If left empty, uses transform.root at Awake.")]
    [SerializeField] private GameObject teapotRoot;

    [Tooltip("The perch transform this existing candle should live on after it is detached from the teapot.")]
    [SerializeField] private Transform perchAnchor;

    [Tooltip("Root candle sprite renderer (the unlit candle that should remain visible after brewing).")]
    [SerializeField] private SpriteRenderer rootCandleSpriteRenderer;

    [Tooltip("Child object that contains the flame/lit animation.")]
    [SerializeField] private GameObject animationChild;

    [Tooltip("Animator on the animation child. Auto-found if left empty.")]
    [SerializeField] private Animator candleAnimator;

    [Header("Teacup Detection")]
    [Tooltip("Any active object whose name contains this text counts as a teacup.")]
    [SerializeField] private string teacupNameContains = "Teacup";

    [Tooltip("How often to check whether any teacup still exists.")]
    [SerializeField] private float teacupPollInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool hasExtinguished = false;
    private float nextPollTime = 0f;

    private void Awake()
    {
        if (teapotRoot == null)
            teapotRoot = transform.root.gameObject;

        if (rootCandleSpriteRenderer == null)
            rootCandleSpriteRenderer = GetComponent<SpriteRenderer>();

        if (animationChild == null)
        {
            Transform child = transform.Find("Animation");
            if (child != null)
                animationChild = child.gameObject;
        }

        if (candleAnimator == null && animationChild != null)
            candleAnimator = animationChild.GetComponent<Animator>();

        // Reparent THIS EXISTING candle to the perch anchor so it survives teapot destruction.
        // This does NOT create a second candle.
        if (perchAnchor != null)
        {
            transform.SetParent(perchAnchor, true);

            if (debugLogs)
                Debug.Log($"[CandleController] Reparented existing candle '{name}' to perch anchor '{perchAnchor.name}'.");
        }
        else
        {
            Debug.LogWarning($"[CandleController] No perchAnchor assigned on '{name}'. Candle will stay where it is.");
        }

        // Root candle base should always remain visible.
        if (rootCandleSpriteRenderer != null)
            rootCandleSpriteRenderer.enabled = true;

        StartLitAnimation();

        if (debugLogs)
        {
            Debug.Log($"[CandleController] Awake on '{name}'. teapotRoot={(teapotRoot != null ? teapotRoot.name : "NULL")}, perchAnchor={(perchAnchor != null ? perchAnchor.name : "NULL")}, animationChild={(animationChild != null ? animationChild.name : "NULL")}, animator={(candleAnimator != null ? candleAnimator.name : "NULL")}");
        }
    }

    private void Update()
    {
        // When teapot is destroyed, extinguish once.
        if (!hasExtinguished && teapotRoot == null)
        {
            if (debugLogs)
                Debug.Log($"[CandleController] Teapot destroyed. Extinguishing candle '{name}'.");

            Extinguish();
            hasExtinguished = true;
        }

        // After extinguishing, wait until no teacup exists anywhere.
        if (hasExtinguished && Time.time >= nextPollTime)
        {
            nextPollTime = Time.time + Mathf.Max(0.02f, teacupPollInterval);

            if (!DoesAnyTeacupExist())
            {
                if (debugLogs)
                    Debug.Log($"[CandleController] No teacup remains. Destroying candle '{name}'.");

                Destroy(gameObject);
            }
        }
    }

    private void StartLitAnimation()
    {
        if (animationChild != null && !animationChild.activeSelf)
            animationChild.SetActive(true);

        if (candleAnimator != null)
        {
            candleAnimator.enabled = true;
            candleAnimator.Play(0, 0, 0f);
            candleAnimator.Update(0f);

            if (debugLogs)
                Debug.Log($"[CandleController] Started lit animation on '{name}'.");
        }
        else if (debugLogs)
        {
            Debug.LogWarning($"[CandleController] No animator found for '{name}'.");
        }
    }

    private void Extinguish()
    {
        // Turn off the lit/animated child completely.
        if (animationChild != null)
            animationChild.SetActive(false);

        // Keep the root candle base visible.
        if (rootCandleSpriteRenderer != null)
            rootCandleSpriteRenderer.enabled = true;

        if (debugLogs)
            Debug.Log($"[CandleController] Extinguished candle '{name}'. Animation child off; root candle stays visible.");
    }

    private bool DoesAnyTeacupExist()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject obj = allObjects[i];
            if (obj == null)
                continue;

            if (obj.name.Contains(teacupNameContains))
                return true;
        }

        return false;
    }

    public void RemoveCandleNow()
    {
        if (debugLogs)
            Debug.Log($"[CandleController] Removing candle '{name}' immediately.");

        Destroy(gameObject);
    }
}