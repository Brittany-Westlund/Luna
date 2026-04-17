using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class IntroBookSpreadSequenceController : MonoBehaviour
{
    [Header("Book Spreads (assign in order)")]
    [SerializeField] private List<GameObject> spreads = new List<GameObject>();

    [Header("Optional Visual Fade Target")]
    [Tooltip("Assign the overall root CanvasGroup for the book visuals if you want them to fade.")]
    [SerializeField] private CanvasGroup visualsCanvasGroup;

    [Header("Optional Music To Fade")]
    [SerializeField] private List<AudioSource> musicSources = new List<AudioSource>();

    [Header("Input")]
    [SerializeField] private KeyCode nextPageKey1 = KeyCode.RightArrow;
    [SerializeField] private KeyCode nextPageKey2 = KeyCode.D;
    [SerializeField] private KeyCode previousPageKey1 = KeyCode.LeftArrow;
    [SerializeField] private KeyCode previousPageKey2 = KeyCode.A;
    [SerializeField] private KeyCode confirmKey = KeyCode.E;

    [Header("Page Turning")]
    [SerializeField] private float pageTurnCooldown = 0.12f;
    [SerializeField] private int startSpreadIndex = 0;

    [Header("End Sequence")]
    [Tooltip("Only pressing E while on the final spread will begin the exit sequence.")]
    [SerializeField] private bool requireLastSpreadForConfirm = true;

    [Tooltip("Delay after pressing E before visual/audio fade starts.")]
    [SerializeField] private float preFadeDelay = 0.75f;

    [Tooltip("Duration of the book/music fade before handing off to LevelManager.")]
    [SerializeField] private float fadeDuration = 1.25f;

    [Tooltip("Optional wait after fade before asking LevelManager to change scenes.")]
    [SerializeField] private float postFadeDelay = 0f;

    [Header("Next Level")]
    [Tooltip("Scene name to pass to LevelManager.Instance.GotoLevel(...)")]
    [SerializeField] private string nextLevelName = "";

    [Tooltip("If true, calls SetNextLevel + GotoNextLevel. If false, calls GotoLevel(nextLevelName).")]
    [SerializeField] private bool useSetNextLevelThenGotoNext = false;

    [Header("LevelManager Handoff")]
    [Tooltip("Pass false if you do NOT want LevelManager to do its own fade when changing levels.")]
    [SerializeField] private bool levelManagerFadeOut = true;

    [Tooltip("Pass true if you want LevelManager to trigger its usual save before loading.")]
    [SerializeField] private bool levelManagerSave = true;

    private int currentSpreadIndex = 0;
    private bool isEnding = false;
    private float nextAllowedPageTurnTime = 0f;
    private readonly Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();

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

        ShowSpread(startSpreadIndex);
    }

    private void Update()
    {
        if (isEnding)
        {
            return;
        }

        HandlePageInput();
        HandleConfirmInput();
    }

    private void HandlePageInput()
    {
        if (Time.unscaledTime < nextAllowedPageTurnTime)
        {
            return;
        }

        if (Input.GetKeyDown(nextPageKey1) || Input.GetKeyDown(nextPageKey2))
        {
            NextSpread();
            nextAllowedPageTurnTime = Time.unscaledTime + pageTurnCooldown;
            return;
        }

        if (Input.GetKeyDown(previousPageKey1) || Input.GetKeyDown(previousPageKey2))
        {
            PreviousSpread();
            nextAllowedPageTurnTime = Time.unscaledTime + pageTurnCooldown;
            return;
        }
    }

    private void HandleConfirmInput()
    {
        if (!Input.GetKeyDown(confirmKey))
        {
            return;
        }

        if (requireLastSpreadForConfirm && !IsOnLastSpread())
        {
            return;
        }

        StartCoroutine(BeginEndSequence());
    }

    public void NextSpread()
    {
        if (spreads == null || spreads.Count == 0)
        {
            return;
        }

        int nextIndex = Mathf.Clamp(currentSpreadIndex + 1, 0, spreads.Count - 1);
        ShowSpread(nextIndex);
    }

    public void PreviousSpread()
    {
        if (spreads == null || spreads.Count == 0)
        {
            return;
        }

        int previousIndex = Mathf.Clamp(currentSpreadIndex - 1, 0, spreads.Count - 1);
        ShowSpread(previousIndex);
    }

    public void ShowSpread(int index)
    {
        if (spreads == null || spreads.Count == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, spreads.Count - 1);
        currentSpreadIndex = index;

        for (int i = 0; i < spreads.Count; i++)
        {
            if (spreads[i] != null)
            {
                spreads[i].SetActive(i == currentSpreadIndex);
            }
        }
    }

    public bool IsOnLastSpread()
    {
        return spreads != null && spreads.Count > 0 && currentSpreadIndex == spreads.Count - 1;
    }

    private IEnumerator BeginEndSequence()
    {
        if (isEnding)
        {
            yield break;
        }

        isEnding = true;

        if (preFadeDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(preFadeDelay);
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = 1f - t;

            if (visualsCanvasGroup != null)
            {
                visualsCanvasGroup.alpha = alpha;
            }

            for (int i = 0; i < musicSources.Count; i++)
            {
                AudioSource source = musicSources[i];
                if (source == null)
                {
                    continue;
                }

                float originalVolume = originalVolumes.ContainsKey(source) ? originalVolumes[source] : 1f;
                source.volume = originalVolume * alpha;
            }

            yield return null;
        }

        if (visualsCanvasGroup != null)
        {
            visualsCanvasGroup.alpha = 0f;
            visualsCanvasGroup.blocksRaycasts = false;
            visualsCanvasGroup.interactable = false;
        }

        for (int i = 0; i < musicSources.Count; i++)
        {
            if (musicSources[i] != null)
            {
                musicSources[i].volume = 0f;
            }
        }

        if (postFadeDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(postFadeDelay);
        }

        HandoffToLevelManager();
    }

    private void HandoffToLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("BookSpreadSequenceController: No LevelManager.Instance found.");
            return;
        }

        if (useSetNextLevelThenGotoNext)
        {
            if (string.IsNullOrWhiteSpace(nextLevelName))
            {
                Debug.LogWarning("BookSpreadSequenceController: nextLevelName is empty, cannot call SetNextLevel/GotoNextLevel.");
                return;
            }

            LevelManager.Instance.SetNextLevel(nextLevelName);
            LevelManager.Instance.GotoNextLevel();
            return;
        }

        if (string.IsNullOrWhiteSpace(nextLevelName))
        {
            Debug.LogWarning("BookSpreadSequenceController: nextLevelName is empty, cannot call GotoLevel.");
            return;
        }

        LevelManager.Instance.GotoLevel(nextLevelName, levelManagerFadeOut, levelManagerSave);
    }

    private void CacheAudioVolumes()
    {
        originalVolumes.Clear();

        if (musicSources == null)
        {
            return;
        }

        for (int i = 0; i < musicSources.Count; i++)
        {
            AudioSource source = musicSources[i];
            if (source != null && !originalVolumes.ContainsKey(source))
            {
                originalVolumes.Add(source, source.volume);
            }
        }
    }
}