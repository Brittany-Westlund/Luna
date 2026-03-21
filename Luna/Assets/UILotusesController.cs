using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILotusesController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("If left empty, this script will look for a child named 'Canvas'.")]
    [SerializeField] private Transform lotusContainer;

    [Header("Fade In")]
    [SerializeField] private float startDelay = 0.3f;
    [SerializeField] private float fadeDuration = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float dimAlpha = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float brightAlpha = 1f;

    [Header("Sparkle")]
    [SerializeField] private float sparkleDuration = 1f;
    [SerializeField] private AudioClip sparkleSFX;
    [Range(0f, 1f)]
    [SerializeField] private float sparkleSFXVolume = 1f;
    [SerializeField] private AudioSource sparkleAudioSource;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly List<LotusUIEntry> lotuses = new List<LotusUIEntry>();
    private readonly HashSet<int> litIndices = new HashSet<int>();

    private Coroutine fadeRoutine;
    private bool isInitialFadeRunning = false;
    private bool initialFadeComplete = false;

    private void Awake()
    {
        AutoAssignContainer();
        CacheLotuses();
        SetAllLotusAlphaImmediate(startAlpha);
        SetAllSparklesActive(false);
    }

    private void Start()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeInToTargetStates());
    }

    private void AutoAssignContainer()
    {
        if (lotusContainer != null)
            return;

        Transform canvasChild = transform.Find("Canvas");
        lotusContainer = canvasChild != null ? canvasChild : transform;

        if (debugLogs)
        {
            Debug.Log($"[UILotusesController] Using lotus container: {lotusContainer.name}");
        }
    }

    private void CacheLotuses()
    {
        lotuses.Clear();

        if (lotusContainer == null)
        {
            Debug.LogWarning("[UILotusesController] No lotus container assigned or found.");
            return;
        }

        for (int i = 0; i < lotusContainer.childCount; i++)
        {
            Transform lotusRoot = lotusContainer.GetChild(i);

            Image rootImage = lotusRoot.GetComponent<Image>();

            Transform sparklesRoot = lotusRoot.Find("Sparkles");
            if (sparklesRoot == null)
            {
                sparklesRoot = lotusRoot.Find("Sparkles (1)");
            }

            lotuses.Add(new LotusUIEntry
            {
                root = lotusRoot,
                rootImage = rootImage,
                sparklesRoot = sparklesRoot != null ? sparklesRoot.gameObject : null
            });

            if (debugLogs)
            {
                Debug.Log($"[UILotusesController] Cached '{lotusRoot.name}' | has Image: {rootImage != null} | has Sparkles: {sparklesRoot != null}");
            }
        }
    }

    private IEnumerator FadeInToTargetStates()
    {
        isInitialFadeRunning = false;
        initialFadeComplete = false;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        isInitialFadeRunning = true;

        if (fadeDuration <= 0f)
        {
            RefreshAllLotusTargetsImmediate();
            isInitialFadeRunning = false;
            initialFadeComplete = true;

            if (debugLogs)
            {
                Debug.Log("[UILotusesController] Fade-in complete instantly.");
            }

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            for (int i = 0; i < lotuses.Count; i++)
            {
                float targetAlpha = litIndices.Contains(i) ? brightAlpha : dimAlpha;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                SetLotusAlpha(i, alpha);
            }

            yield return null;
        }

        RefreshAllLotusTargetsImmediate();

        isInitialFadeRunning = false;
        initialFadeComplete = true;
        fadeRoutine = null;

        if (debugLogs)
        {
            Debug.Log("[UILotusesController] Fade-in complete.");
        }
    }

    // For scene restore: mark as lit.
    // If initial fade is already done, brighten immediately.
    // If fade is still running, the fade routine will pick it up automatically.
    public void RegisterLitLotus(int index)
    {
        if (!IsValidIndex(index))
            return;

        litIndices.Add(index);

        if (initialFadeComplete)
        {
            SetLotusAlpha(index, brightAlpha);
        }

        if (debugLogs)
        {
            Debug.Log($"[UILotusesController] Registered lit lotus index {index} ({lotuses[index].root.name}) | fadeRunning={isInitialFadeRunning} | fadeComplete={initialFadeComplete}");
        }
    }

    // For real-time collection: mark as lit AND brighten immediately.
    public void LightLotus(int index)
    {
        if (!IsValidIndex(index))
            return;

        litIndices.Add(index);
        SetLotusAlpha(index, brightAlpha);

        if (debugLogs)
        {
            Debug.Log($"[UILotusesController] Lit lotus index {index} ({lotuses[index].root.name})");
        }
    }

    public void DimLotus(int index)
    {
        if (!IsValidIndex(index))
            return;

        litIndices.Remove(index);

        if (initialFadeComplete)
        {
            SetLotusAlpha(index, dimAlpha);
        }

        if (debugLogs)
        {
            Debug.Log($"[UILotusesController] Dimmed lotus index {index} ({lotuses[index].root.name})");
        }
    }

    public void TriggerSparkle(int index)
    {
        if (!IsValidIndex(index))
            return;

        GameObject sparklesRoot = lotuses[index].sparklesRoot;
        if (sparklesRoot == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[UILotusesController] No Sparkles object found on lotus index {index} ({lotuses[index].root.name})");
            }
            return;
        }

        StartCoroutine(SparkleRoutine(sparklesRoot));

        if (debugLogs)
        {
            Debug.Log($"[UILotusesController] Triggered sparkle for lotus index {index} ({lotuses[index].root.name})");
        }
    }

    private IEnumerator SparkleRoutine(GameObject sparklesRoot)
    {
        sparklesRoot.SetActive(true);
        PlaySparkleSFX();

        yield return new WaitForSeconds(sparkleDuration);

        sparklesRoot.SetActive(false);
    }

    private void PlaySparkleSFX()
    {
        if (sparkleSFX == null || sparkleAudioSource == null)
            return;

        sparkleAudioSource.PlayOneShot(sparkleSFX, sparkleSFXVolume);
    }

    private void RefreshAllLotusTargetsImmediate()
    {
        for (int i = 0; i < lotuses.Count; i++)
        {
            float targetAlpha = litIndices.Contains(i) ? brightAlpha : dimAlpha;
            SetLotusAlpha(i, targetAlpha);
        }
    }

    private void SetLotusAlpha(int index, float alpha)
    {
        if (!IsValidIndex(index))
            return;

        Image img = lotuses[index].rootImage;
        if (img == null)
            return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetAllLotusAlphaImmediate(float alpha)
    {
        for (int i = 0; i < lotuses.Count; i++)
        {
            SetLotusAlpha(i, alpha);
        }
    }

    private void SetAllSparklesActive(bool isActive)
    {
        for (int i = 0; i < lotuses.Count; i++)
        {
            if (lotuses[i].sparklesRoot != null)
            {
                lotuses[i].sparklesRoot.SetActive(isActive);
            }
        }
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= lotuses.Count)
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[UILotusesController] Index out of range: {index}");
            }
            return false;
        }

        return true;
    }

    [System.Serializable]
    private class LotusUIEntry
    {
        public Transform root;
        public Image rootImage;
        public GameObject sparklesRoot;
    }
}