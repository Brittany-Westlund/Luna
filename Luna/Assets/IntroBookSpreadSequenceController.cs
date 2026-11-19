
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.CorgiEngine;

public class IntroBookSpreadSequenceController : MonoBehaviour
{
    [Serializable]
    private class ColorShift
    {
        [Min(0f)] public float delay = 0f;
        [Min(0f)] public float duration = 1f;
        public Color targetColor = Color.white;
    }

    [Serializable]
    private class BloomItem
    {
        public Graphic graphic;

        // Original field names retained for Unity serialization.
        [HideInInspector] public Color targetColor = Color.white;
        [HideInInspector, Min(0f)] public float delay = 0.5f;
        [HideInInspector, Min(0f)] public float duration = 2f;

        public Color startingColor = new Color(1f, 1f, 1f, 0f);
        public List<ColorShift> shifts = new List<ColorShift>();
    }

    [Serializable]
    private class SpreadBloom
    {
        // Retained for serialization, but not used for playback.
        [HideInInspector] public bool enabled = true;
        public List<BloomItem> items = new List<BloomItem>();
    }

    [Min(0f)]
    [SerializeField] private float bookControllerStartDelay = 0f;
    
    [Header("Book Spreads")]
    [SerializeField] private List<GameObject> spreads = new List<GameObject>();

    [Header("Spread Blooms (same order as spreads)")]
    [SerializeField] private List<SpreadBloom> spreadBlooms = new List<SpreadBloom>();

    [Header("Page Turning")]
    [SerializeField] private int startSpreadIndex = 0;
    [Min(0f)] [SerializeField] private float pageTurnCooldown = 0.12f;

    // Assigned artwork fades for exactly one second.
    private const float PageTurnFadeDuration = 1f;

    [Header("Page Turn Sound Effect")]
    [SerializeField] private AudioSource pageTurnSFXSource;
    [SerializeField] private AudioClip pageTurnSFXClip;
    [Range(0f, 1f)] [SerializeField] private float pageTurnSFXVolume = 1f;

    [Header("Optional Visual Fade Target (Continue only)")]
    [SerializeField] private CanvasGroup visualsCanvasGroup;

    [Header("Optional Music To Fade (Continue only)")]
    [SerializeField] private List<AudioSource> musicSources = new List<AudioSource>();

    [Header("Continue Sound Effect")]
    [SerializeField] private AudioSource continueSFXSource;
    [SerializeField] private AudioClip continueSFXClip;

    [Header("Disable On Continue")]
    [SerializeField] private GameObject objectToDisableOnContinue;

    [Header("Input")]
    [SerializeField] private KeyCode nextPageKey1 = KeyCode.RightArrow;
    [SerializeField] private KeyCode nextPageKey2 = KeyCode.D;
    [SerializeField] private KeyCode previousPageKey1 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode previousPageKey2 = KeyCode.A;
    [SerializeField] private KeyCode confirmKey = KeyCode.W;

    [Header("End Sequence")]
    [SerializeField] private bool requireLastSpreadForConfirm = true;
    [Min(0f)] [SerializeField] private float preFadeDelay = 0.75f;
    [Min(0f)] [SerializeField] private float fadeDuration = 1.25f;
    [Min(0f)] [SerializeField] private float postFadeDelay = 0f;

    [Header("Next Level")]
    [SerializeField] private string nextLevelName = "";
    [SerializeField] private bool useSetNextLevelThenGotoNext = false;

    [Header("LevelManager Handoff")]
    [SerializeField] private bool levelManagerFadeOut = true;
    [SerializeField] private bool levelManagerSave = true;

    [Header("Debugging")]
    [SerializeField] private bool enableDebugLogs = true;

    private int currentSpreadIndex;
    private bool isEnding;
    private bool isTurningPage;
    private float nextAllowedPageTurnTime;

    private readonly List<Coroutine> activeBloomCoroutines =
        new List<Coroutine>();

    private readonly Dictionary<AudioSource, float> originalVolumes =
        new Dictionary<AudioSource, float>();

    private void Awake()
    {
        CacheAudioVolumes();
    }

    private void Start()
    {
        if (visualsCanvasGroup != null)
        {
            visualsCanvasGroup.alpha = 1f;
            visualsCanvasGroup.blocksRaycasts = true;
            visualsCanvasGroup.interactable = true;
        }

        if (spreads == null || spreads.Count == 0)
        {
            Debug.LogWarning("[IntroBook] No spreads assigned.", this);
            return;
        }

        currentSpreadIndex = Mathf.Clamp(
            startSpreadIndex,
            0,
            spreads.Count - 1
        );

        ActivateSpread(currentSpreadIndex);
        StartSpreadBloom(currentSpreadIndex);
    }

    private void Update()
    {
        if (isEnding) return;

        if (!isTurningPage &&
            Time.unscaledTime >= nextAllowedPageTurnTime)
        {
            if (Input.GetKeyDown(nextPageKey1) ||
                Input.GetKeyDown(nextPageKey2))
            {
                NextSpread();
            }
            else if (Input.GetKeyDown(previousPageKey1) ||
                     Input.GetKeyDown(previousPageKey2))
            {
                PreviousSpread();
            }
        }

        if (Input.GetKeyDown(confirmKey))
            Continue();
    }

    public void NextSpread()
    {
        ShowSpread(currentSpreadIndex + 1);
    }

    public void PreviousSpread()
    {
        ShowSpread(currentSpreadIndex - 1);
    }

    public void ShowSpread(int index)
    {
        if (isEnding || isTurningPage ||
            spreads == null || spreads.Count == 0)
            return;

        if (Time.unscaledTime < nextAllowedPageTurnTime)
            return;

        if (index < 0 ||
            index >= spreads.Count ||
            index == currentSpreadIndex)
            return;

        StartCoroutine(TurnPage(index));
    }

    private IEnumerator TurnPage(int targetIndex)
    {
        isTurningPage = true;

        StopActiveBlooms();
        PlayPageTurnSFX();

        // Fade only assigned artwork, never the book itself.
        List<Graphic> outgoing =
            GetSpreadBloomGraphics(currentSpreadIndex);

        List<Color> originalColors =
            new List<Color>(outgoing.Count);

        foreach (Graphic graphic in outgoing)
        {
            originalColors.Add(
                graphic != null ? graphic.color : Color.clear
            );
        }

        float elapsed = 0f;

        while (elapsed < PageTurnFadeDuration)
        {
            elapsed = Mathf.Min(
                PageTurnFadeDuration,
                elapsed + Time.unscaledDeltaTime
            );

            float t = elapsed / PageTurnFadeDuration;

            for (int i = 0; i < outgoing.Count; i++)
            {
                if (outgoing[i] == null) continue;

                Color color = originalColors[i];

                color.a = Mathf.Lerp(
                    originalColors[i].a,
                    0f,
                    t
                );

                outgoing[i].color = color;
            }

            yield return null;
        }

        foreach (Graphic graphic in outgoing)
        {
            if (graphic == null) continue;

            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
        }

        currentSpreadIndex = targetIndex;

        ActivateSpread(currentSpreadIndex);
        StartSpreadBloom(currentSpreadIndex);

        nextAllowedPageTurnTime =
            Time.unscaledTime + Mathf.Max(0f, pageTurnCooldown);

        isTurningPage = false;
    }

    private void ActivateSpread(int index)
    {
        for (int i = 0; i < spreads.Count; i++)
        {
            if (spreads[i] != null)
                spreads[i].SetActive(i == index);
        }

        Log(
            "Showing spread " +
            (index + 1) +
            " / " +
            spreads.Count
        );
    }

    private List<Graphic> GetSpreadBloomGraphics(int spreadIndex)
    {
        List<Graphic> result = new List<Graphic>();

        if (spreadBlooms == null ||
            spreadIndex < 0 ||
            spreadIndex >= spreadBlooms.Count)
            return result;

        SpreadBloom bloom = spreadBlooms[spreadIndex];

        if (bloom == null || bloom.items == null)
            return result;

        foreach (BloomItem item in bloom.items)
        {
            if (item != null &&
                item.graphic != null &&
                !result.Contains(item.graphic))
            {
                result.Add(item.graphic);
            }
        }

        return result;
    }

    private void StartSpreadBloom(int spreadIndex)
    {
        StopActiveBlooms();

        if (spreadBlooms == null ||
            spreadIndex < 0 ||
            spreadIndex >= spreadBlooms.Count)
            return;

        SpreadBloom bloom = spreadBlooms[spreadIndex];

        if (bloom == null || bloom.items == null)
            return;

        // Reset all assigned artwork before starting animations.
        HashSet<Graphic> initialized = new HashSet<Graphic>();

        foreach (BloomItem item in bloom.items)
        {
            if (item == null ||
                item.graphic == null ||
                !initialized.Add(item.graphic))
                continue;

            item.graphic.color =
                item.shifts != null && item.shifts.Count > 0
                ? item.startingColor
                : LegacyStartingColor(item);
        }

        // One coroutine per Graphic prevents competing animations.
        foreach (Graphic graphic in initialized)
        {
            List<BloomItem> entries = new List<BloomItem>();

            foreach (BloomItem item in bloom.items)
            {
                if (item != null && item.graphic == graphic)
                    entries.Add(item);
            }

            activeBloomCoroutines.Add(
                StartCoroutine(
                    AnimateGraphicEntries(graphic, entries)
                )
            );
        }
    }

    private static Color LegacyStartingColor(BloomItem item)
    {
        Color color = item.targetColor;
        color.a = 0f;
        return color;
    }

    private IEnumerator AnimateGraphicEntries(
        Graphic graphic,
        List<BloomItem> entries)
    {
        foreach (BloomItem item in entries)
        {
            if (graphic == null)
                yield break;

            if (item.shifts != null && item.shifts.Count > 0)
            {
                foreach (ColorShift shift in item.shifts)
                {
                    if (shift == null)
                        continue;

                    yield return AnimateShift(
                        graphic,
                        shift.delay,
                        shift.duration,
                        shift.targetColor
                    );

                    if (graphic == null)
                        yield break;
                }
            }
            else
            {
                // Preserve playback of older single-shift items.
                yield return AnimateShift(
                    graphic,
                    item.delay,
                    item.duration,
                    item.targetColor
                );

                if (graphic == null)
                    yield break;
            }
        }
    }

    private IEnumerator AnimateShift(
        Graphic graphic,
        float delay,
        float duration,
        Color target)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (graphic == null)
            yield break;

        Color from = graphic.color;
        float length = Mathf.Max(0f, duration);

        if (length <= 0f)
        {
            graphic.color = target;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < length)
        {
            if (graphic == null)
                yield break;

            elapsed = Mathf.Min(
                length,
                elapsed + Time.unscaledDeltaTime
            );

            graphic.color = Color.Lerp(
                from,
                target,
                elapsed / length
            );

            yield return null;
        }

        if (graphic != null)
            graphic.color = target;
    }

    private void StopActiveBlooms()
    {
        foreach (Coroutine routine in activeBloomCoroutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        activeBloomCoroutines.Clear();
    }

    private void PlayPageTurnSFX()
    {
        if (pageTurnSFXSource != null &&
            pageTurnSFXClip != null)
        {
            pageTurnSFXSource.PlayOneShot(
                pageTurnSFXClip,
                pageTurnSFXVolume
            );
        }
    }

    public bool IsOnLastSpread()
    {
        return spreads != null &&
               spreads.Count > 0 &&
               currentSpreadIndex == spreads.Count - 1;
    }

    public void Continue()
    {
        if (isEnding || isTurningPage)
            return;

        if (spreads == null || spreads.Count == 0)
            return;

        if (requireLastSpreadForConfirm && !IsOnLastSpread())
            return;

        if (string.IsNullOrWhiteSpace(nextLevelName))
        {
            Debug.LogError(
                "[IntroBook] Next Level Name is empty.",
                this
            );
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextLevelName))
        {
            Debug.LogError(
                "[IntroBook] Scene not in build or not loadable: " +
                nextLevelName,
                this
            );
            return;
        }

        StopActiveBlooms();

        if (objectToDisableOnContinue != null)
            objectToDisableOnContinue.SetActive(false);

        if (continueSFXSource != null &&
            continueSFXClip != null)
        {
            continueSFXSource.PlayOneShot(continueSFXClip);
        }

        StartCoroutine(BeginEndSequence());
    }

    private IEnumerator BeginEndSequence()
    {
        if (isEnding)
            yield break;

        isEnding = true;

        if (preFadeDelay > 0f)
            yield return new WaitForSecondsRealtime(preFadeDelay);

        float length = Mathf.Max(0f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < length)
        {
            elapsed = Mathf.Min(
                length,
                elapsed + Time.unscaledDeltaTime
            );

            ApplyFade(1f - elapsed / length);

            yield return null;
        }

        ApplyFade(0f);

        if (visualsCanvasGroup != null)
        {
            visualsCanvasGroup.blocksRaycasts = false;
            visualsCanvasGroup.interactable = false;
        }

        if (postFadeDelay > 0f)
            yield return new WaitForSecondsRealtime(postFadeDelay);

        HandoffToLevelManager();
    }

    private void ApplyFade(float alpha)
    {
        if (visualsCanvasGroup != null)
            visualsCanvasGroup.alpha = alpha;

        if (musicSources == null)
            return;

        foreach (AudioSource source in musicSources)
        {
            if (source == null)
                continue;

            float original;

            if (!originalVolumes.TryGetValue(source, out original))
                original = source.volume;

            source.volume = original * alpha;
        }
    }

    private void HandoffToLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError(
                "[IntroBook] LevelManager.Instance is missing.",
                this
            );
            return;
        }

        if (useSetNextLevelThenGotoNext)
        {
            LevelManager.Instance.SetNextLevel(nextLevelName);
            LevelManager.Instance.GotoNextLevel();
        }
        else
        {
            LevelManager.Instance.GotoLevel(
                nextLevelName,
                levelManagerFadeOut,
                levelManagerSave
            );
        }
    }

    private void CacheAudioVolumes()
    {
        originalVolumes.Clear();

        if (musicSources == null)
            return;

        foreach (AudioSource source in musicSources)
        {
            if (source != null &&
                !originalVolumes.ContainsKey(source))
            {
                originalVolumes.Add(source, source.volume);
            }
        }
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log("[IntroBook] " + message, this);
    }
}
