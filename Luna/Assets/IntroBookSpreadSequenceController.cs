
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.CorgiEngine;

public class IntroBookSpreadSequenceController : MonoBehaviour
{
    public enum ArtworkAnimation
    {
        None,
        Fade,
        ColorShifts,
        Sequence
    }

    public enum AnimationStepType
    {
        Fade,
        ColorShift
    }


    public enum SpecialAnimation { None, Dew, Steam }
    public enum ArtworkEvent { Started, Completed, SpecialStarted, SpecialCompleted }
    public enum PromptTiming { BeforeArtwork, AfterArtwork }

    [Serializable]
    public class ArtworkDependency
    {
        [Tooltip("Name of another artwork entry on this spread. Names must be unique.")]
        public string artworkName = "";
        public ArtworkEvent waitFor = ArtworkEvent.Completed;
    }

    [Serializable]
    public class InteractKey
    {
        public string name = "New Interaction";
        public KeyCode key = KeyCode.E;
        public Graphic promptGraphic;
        public CanvasGroup promptCanvasGroup;
        public PromptTiming promptTiming = PromptTiming.BeforeArtwork;
        [Tooltip("Required for After Artwork. Name of an artwork entry on this spread.")]
        public string afterArtwork = "";
        public ArtworkEvent afterEvent = ArtworkEvent.Completed;
        [Min(0f)] public float promptDelay = 0f;
        [Min(0f)] public float promptFadeIn = 0.25f;
        [Min(0f)] public float promptFadeOut = 0.25f;
        public AudioClip pressSFX;
        [Range(0f, 1f)] public float pressVolume = 1f;
    }

    [Serializable]
    public class ColorShift
    {
        [Min(0f)] public float delay = 0f;
        [Min(0f)] public float duration = 1f;
        public Color targetColor = Color.white;
    }

    [Serializable]
    public class AnimationStep
    {
        public AnimationStepType type = AnimationStepType.Fade;

        [Min(0f)]
        public float delay = 0f;

        [Min(0f)]
        public float duration = 1f;

        [Range(0f, 1f)]
        public float targetAlpha = 1f;

        public Color targetColor = Color.white;
    }

    [Serializable]
    public class ArtworkEntry
    {
        public string name = "Artwork";
        public Graphic graphic;

        [Tooltip("Optional: assign a CanvasGroup on a parent to fade all child UI elements together. Leave Graphic empty when using this.")]
        public CanvasGroup canvasGroup;

        public ArtworkAnimation animation = ArtworkAnimation.Fade;

        [Header("Original Fade Settings")]
        [Min(0f)] public float delay = 0f;
        [Min(0f)] public float duration = 1f;
        [Range(0f, 1f)] public float startingAlpha = 0f;
        [Range(0f, 1f)] public float targetAlpha = 1f;

        [Header("Starting Appearance")]
        public Color startingColor = Color.white;

        [Header("Original Color Shifts")]
        public List<ColorShift> shifts = new List<ColorShift>();

        [Header("Sequence Steps")]
        public List<AnimationStep> steps = new List<AnimationStep>();

        [Header("Optional Interaction")]
        public bool waitForInteractKey = false;
        public string interactionName = "";
        [Tooltip("All listed dependencies must be satisfied before this entry begins.")]
        public List<ArtworkDependency> dependencies = new List<ArtworkDependency>();
        [Tooltip("For Dew/Steam: additional dependencies before the special animation begins. " +
                 "These do not delay the artwork's initial fade.")]
        public List<ArtworkDependency> specialDependencies = new List<ArtworkDependency>();

        [Header("Optional Sounds")]
        public AudioClip initialSFX;
        [Range(0f, 1f)] public float initialVolume = 1f;
        [Range(0f, 1f)] public float initialSFXTriggerAlpha = 0.9f;
        public AudioClip ambientSFX;
        [Range(0f, 1f)] public float ambientVolume = 1f;
        [Range(0f, 1f)] public float ambientStartAlpha = 0.9f;

        [Header("Optional Special Animation")]
        public SpecialAnimation specialAnimation = SpecialAnimation.None;
        [Min(0f)] public float dewExitDelay = 0f;
        [Min(0f)] public float dewExitDuration = 1f;
        [Range(0f, 1f)] public float dewEndScale = 0.5f;
        [Min(0f)] public float dewDropDistance = 30f;
        [Tooltip("Image for Steam. When blank, uses Graphic or first Graphic under CanvasGroup.")]
        public Graphic steamGraphic;
        [Range(0f, 1f)] public float steamMinimumAlpha = 0.65f;
        [Range(0f, 1f)] public float steamMaximumAlpha = 1f;
        [Min(0.01f)] public float steamCycleDuration = 2f;
        public float steamPhaseOffset = 0f;
    }

    [Serializable]
    public class BookPage
    {
        public string name = "New Spread";
        public GameObject pageObject;
        [Tooltip("Prefab asset for this page. Its asset name is the saved ID.")]
        public BookPagePrefab pagePrefab;
        public List<ArtworkEntry> artwork = new List<ArtworkEntry>();
        public List<InteractKey> interactKeys = new List<InteractKey>();
        [Tooltip("Stop page turning and Continue until this spread's named interactions have been pressed.")]
        public bool requireInteractionsBeforeTurning = true;
        [Tooltip("Normally completed interactions are skipped when a spread is revisited.")]
        public bool replayInteractionsOnRevisit = false;
        [Tooltip("Optionally keep navigation locked until one artwork event is reached.")]
        public bool waitForArtworkBeforeTurning = false;
        public string unlockArtworkName = "";
        public ArtworkEvent unlockEvent = ArtworkEvent.Completed;
        [Min(0f)] public float unlockDelay = 0f;
    }

    [Serializable]
    public class RevisitArrangementItem
    {
        public string name = "Arrangement Item";
        [Tooltip("Scene object from the first spread. Uses its CanvasGroup, not its Image alpha.")]
        public GameObject target;
        [Min(0f)] public float delay = 0.5f;
        [Min(0f)] public float duration = 1f;
        [Range(0f, 1f)] public float targetAlpha = 1f;
    }

    [Header("Book Pages")]
    [SerializeField]
    private List<BookPage> pages = new List<BookPage>();

    [Header("Previously Visited Pages")]
    [SerializeField] private IntroBookVisitedPagesController visitedPagesController;

    [Header("Introduction Migration")]
    [Tooltip("ON preserves the original intro handoff and first-spread revisit behavior. Turn OFF only after moving the intro's assignments and disabling its component.")]
    [SerializeField] private bool useLegacyIntroduction = true;

    [Header("First Page Tea Arrangement - Revisits Only")]
    [Tooltip("The intro controller is only used to restore Dew's original position and stop unfinished intro fades when leaving.")]
    [SerializeField] private IntroBookSequenceController introController;
    [Tooltip("Configure Lilystool and Dew with the same delay (e.g. 0.5) and duration.")]
    [SerializeField] private List<RevisitArrangementItem> firstSpreadArrangement =
        new List<RevisitArrangementItem>();

    [Header("Page Turning")]
    [SerializeField] private int startSpreadIndex = 0;

    [Min(0f)]
    [SerializeField] private float pageTurnCooldown = 0.12f;

    [Min(0f)]
    [SerializeField] private float pageTurnFadeDuration = 1f;

    [Header("Page Turn Sound Effect")]
    [SerializeField] private AudioSource pageTurnSFXSource;
    [SerializeField] private AudioClip pageTurnSFXClip;

    [Range(0f, 1f)]
    [SerializeField] private float pageTurnSFXVolume = 1f;

    [Header("Optional Visual Fade Target (Continue only)")]
    [SerializeField] private CanvasGroup visualsCanvasGroup;

    [Header("Optional Music To Fade (Continue only)")]
    [SerializeField] private List<AudioSource> musicSources =
        new List<AudioSource>();

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

    [Min(0f)]
    [SerializeField] private float preFadeDelay = 0.75f;

    [Min(0f)]
    [SerializeField] private float fadeDuration = 1.25f;

    [Min(0f)]
    [SerializeField] private float postFadeDelay = 0f;

    [Header("Next Level")]
    [SerializeField] private string nextLevelName = "";

    [SerializeField]
    private bool useSetNextLevelThenGotoNext = false;

    [Header("LevelManager Handoff")]
    [SerializeField] private bool levelManagerFadeOut = true;
    [SerializeField] private bool levelManagerSave = true;

    [Header("Debugging")]
    [SerializeField] private bool enableDebugLogs = true;

    private int currentSpreadIndex;
    private bool isEnding;
    private bool isTurningPage;
    private bool viewingHistory;
    private bool firstSpreadHasBeenLeft;
    private float nextAllowedPageTurnTime;

    private readonly List<Coroutine> activeAnimations =
        new List<Coroutine>();

    private readonly Dictionary<AudioSource, float> originalVolumes =
        new Dictionary<AudioSource, float>();

    private readonly HashSet<ArtworkEntry> startedArtwork = new HashSet<ArtworkEntry>();
    private readonly HashSet<ArtworkEntry> completedArtwork = new HashSet<ArtworkEntry>();
    private readonly HashSet<ArtworkEntry> startedSpecials = new HashSet<ArtworkEntry>();
    private readonly HashSet<ArtworkEntry> completedSpecials = new HashSet<ArtworkEntry>();
    private readonly HashSet<string> firedInteractions = new HashSet<string>();
    private readonly HashSet<int> previouslyCompletedSpreads = new HashSet<int>();
    private readonly Dictionary<ArtworkEntry, List<AudioSource>> artworkSounds =
        new Dictionary<ArtworkEntry, List<AudioSource>>();
    private readonly Dictionary<ArtworkEntry, Vector3> originalScales =
        new Dictionary<ArtworkEntry, Vector3>();
    private readonly Dictionary<ArtworkEntry, Vector3> originalPositions =
        new Dictionary<ArtworkEntry, Vector3>();
    private readonly Dictionary<ArtworkEntry, Vector2> originalAnchors =
        new Dictionary<ArtworkEntry, Vector2>();
    private readonly Dictionary<ArtworkEntry, bool> initialPlayed =
        new Dictionary<ArtworkEntry, bool>();
    private readonly Dictionary<ArtworkEntry, bool> ambientPlayed =
        new Dictionary<ArtworkEntry, bool>();
    private readonly HashSet<string> reportedConfigurationIssues = new HashSet<string>();
    private bool interactionsFinished = true;
    private int artworkRunVersion = 0;
    private float pageTurnUnlockTime = -1f;

    public int CurrentSpreadIndex => currentSpreadIndex;
    public bool ViewingHistory => viewingHistory;
    public bool IsBusy => isTurningPage || isEnding;
    public float PageTurnFadeDuration => pageTurnFadeDuration;
    public float PageTurnCooldown => pageTurnCooldown;

    private void Awake()
    {
        CacheAudioVolumes();
    }

    private void OnDisable()
    {
        StopActiveAnimations();
    }

    private void Start()
    {
        if (visualsCanvasGroup != null)
        {
            visualsCanvasGroup.alpha = 1f;
            visualsCanvasGroup.blocksRaycasts = true;
            visualsCanvasGroup.interactable = true;
        }

        if (pages == null || pages.Count == 0)
        {
            Debug.LogWarning("[IntroBook] No pages assigned.", this);
            return;
        }

        currentSpreadIndex = Mathf.Clamp(
            startSpreadIndex, 0, pages.Count - 1);

        for (int i = 0; i < pages.Count; i++)
            if (i != 0 || !useLegacyIntroduction) InitializePageArtwork(pages[i]);

        ActivateSpread(currentSpreadIndex);
        // Legacy mode retains the original intro's ownership of page zero.
        if (currentSpreadIndex != 0 || !useLegacyIntroduction)
            StartSpreadArtwork(currentSpreadIndex);
        RecordCurrentPage();
    }

    private void Update()
    {
        if (isEnding || viewingHistory)
            return;

        if (!isTurningPage &&
            Time.unscaledTime >= nextAllowedPageTurnTime &&
            CanLeaveCurrentSpread())
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
        if (!CanLeaveCurrentSpread()) return;
        if (currentSpreadIndex == 0 && visitedPagesController != null)
        {
            visitedPagesController.EnterHistory();
            return;
        }

        ShowSpread(currentSpreadIndex - 1);
    }

    public void ShowSpread(int index)
    {
        if (isEnding || isTurningPage || viewingHistory ||
            pages == null || pages.Count == 0)
            return;

        if (Time.unscaledTime < nextAllowedPageTurnTime)
            return;

        if (index < 0 || index >= pages.Count ||
            index == currentSpreadIndex || !CanLeaveCurrentSpread())
            return;

        StartCoroutine(TurnPage(index));
    }

    private IEnumerator TurnPage(int targetIndex)
    {
        isTurningPage = true;
        StopActiveAnimations();
        PlayPageTurnSFX();

        BookPage outgoingPage = pages[currentSpreadIndex];
        bool leavingFirstSpread = currentSpreadIndex == 0;
        if (leavingFirstSpread)
        {
            firstSpreadHasBeenLeft = true;
            if (useLegacyIntroduction && introController != null)
                introController.OnIntroductionPageLeft();
        }

        List<Graphic> outgoing = GetPageGraphics(outgoingPage);
        List<CanvasGroup> outgoingGroups = GetPageCanvasGroups(outgoingPage);
        List<float> outgoingGroupStarts = new List<float>();
        foreach (CanvasGroup group in outgoingGroups)
            outgoingGroupStarts.Add(group.alpha);

        List<CanvasGroup> arrangementGroups = leavingFirstSpread && useLegacyIntroduction
            ? GetArrangementGroups() : new List<CanvasGroup>();
        List<float> arrangementStarts = new List<float>();
        foreach (CanvasGroup group in arrangementGroups)
            arrangementStarts.Add(group.alpha);
        List<Color> originalColors = new List<Color>();

        foreach (Graphic graphic in outgoing)
        {
            originalColors.Add(
                graphic != null ? graphic.color : Color.clear);
        }

        float duration = Mathf.Max(0f, pageTurnFadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed = Mathf.Min(
                duration, elapsed + Time.unscaledDeltaTime);

            float t = duration <= 0f
                ? 1f
                : elapsed / duration;

            for (int i = 0; i < outgoing.Count; i++)
            {
                if (outgoing[i] == null)
                    continue;

                Color color = originalColors[i];
                color.a = Mathf.Lerp(
                    originalColors[i].a, 0f, t);

                outgoing[i].color = color;
            }

            for (int i = 0; i < outgoingGroups.Count; i++)
                if (outgoingGroups[i] != null)
                    outgoingGroups[i].alpha = Mathf.Lerp(outgoingGroupStarts[i], 0f, t);

            for (int i = 0; i < arrangementGroups.Count; i++)
                if (arrangementGroups[i] != null)
                    arrangementGroups[i].alpha = Mathf.Lerp(arrangementStarts[i], 0f, t);

            yield return null;
        }

        foreach (CanvasGroup group in outgoingGroups)
            if (group != null) group.alpha = 0f;

        foreach (CanvasGroup group in arrangementGroups)
            if (group != null) group.alpha = 0f;

        foreach (Graphic graphic in outgoing)
        {
            if (graphic == null)
                continue;

            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
        }

        currentSpreadIndex = targetIndex;

        ActivateSpread(currentSpreadIndex);
        StartSpreadArtwork(currentSpreadIndex);
        if (useLegacyIntroduction && currentSpreadIndex == 0 && firstSpreadHasBeenLeft)
            StartFirstSpreadArrangement();
        RecordCurrentPage();

        nextAllowedPageTurnTime =
            Time.unscaledTime + Mathf.Max(0f, pageTurnCooldown);

        isTurningPage = false;
    }

    private void RecordCurrentPage()
    {
        if (pages == null || currentSpreadIndex < 0 ||
            currentSpreadIndex >= pages.Count)
            return;

        BookPage page = pages[currentSpreadIndex];
        if (page != null && page.pagePrefab != null)
            BookPageHistory.MarkVisited(page.pagePrefab.name);
    }

    public bool IsCurrentScenePage(string prefabName)
    {
        if (pages == null)
            return false;

        foreach (BookPage page in pages)
            if (page != null && page.pagePrefab != null &&
                page.pagePrefab.name == prefabName)
                return true;

        return false;
    }

    public void EnterHistoryView()
    {
        if (viewingHistory || isTurningPage || isEnding ||
            currentSpreadIndex != 0 || !CanLeaveCurrentSpread())
            return;

        StartCoroutine(SwitchToHistory());
    }

    private IEnumerator SwitchToHistory()
    {
        isTurningPage = true;
        StopActiveAnimations();
        firstSpreadHasBeenLeft = true;
        if (useLegacyIntroduction && introController != null)
            introController.OnIntroductionPageLeft();
        PlayPageTurnSFX();
        yield return FadeCurrentArtworkOut();
        viewingHistory = true;
        ActivateSpread(-1);
        isTurningPage = false;
    }

    public void ReturnFromHistory()
    {
        if (!viewingHistory || isTurningPage || isEnding)
            return;

        viewingHistory = false;
        currentSpreadIndex = 0;
        ActivateSpread(0);
        StartSpreadArtwork(0);
        if (useLegacyIntroduction)
            StartFirstSpreadArrangement();
        RecordCurrentPage();
        nextAllowedPageTurnTime = Time.unscaledTime + pageTurnCooldown;
    }

    public void PlayHistoryPageTurnSound()
    {
        PlayPageTurnSFX();
    }

    private IEnumerator FadeCurrentArtworkOut()
    {
        List<Graphic> graphics = GetPageGraphics(pages[currentSpreadIndex]);
        List<CanvasGroup> pageGroups = GetPageCanvasGroups(pages[currentSpreadIndex]);
        List<float> pageGroupStarts = new List<float>();
        foreach (CanvasGroup group in pageGroups)
            pageGroupStarts.Add(group.alpha);

        List<CanvasGroup> arrangementGroups = useLegacyIntroduction
            ? GetArrangementGroups() : new List<CanvasGroup>();
        List<float> arrangementStarts = new List<float>();
        foreach (CanvasGroup group in arrangementGroups)
            arrangementStarts.Add(group.alpha);
        List<Color> originals = new List<Color>();
        foreach (Graphic graphic in graphics)
            originals.Add(graphic != null ? graphic.color : Color.clear);

        float duration = Mathf.Max(0f, pageTurnFadeDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed = Mathf.Min(duration, elapsed + Time.unscaledDeltaTime);
            float t = duration <= 0f ? 1f : elapsed / duration;
            for (int i = 0; i < graphics.Count; i++)
            {
                if (graphics[i] == null) continue;
                Color color = originals[i];
                color.a = Mathf.Lerp(originals[i].a, 0f, t);
                graphics[i].color = color;
            }
            for (int i = 0; i < pageGroups.Count; i++)
                if (pageGroups[i] != null)
                    pageGroups[i].alpha = Mathf.Lerp(pageGroupStarts[i], 0f, t);
            for (int i = 0; i < arrangementGroups.Count; i++)
                if (arrangementGroups[i] != null)
                    arrangementGroups[i].alpha = Mathf.Lerp(arrangementStarts[i], 0f, t);
            yield return null;
        }
        foreach (CanvasGroup group in pageGroups)
            if (group != null) group.alpha = 0f;
        foreach (CanvasGroup group in arrangementGroups)
            if (group != null) group.alpha = 0f;
    }

    private List<CanvasGroup> GetArrangementGroups()
    {
        List<CanvasGroup> result = new List<CanvasGroup>();
        foreach (RevisitArrangementItem item in firstSpreadArrangement)
        {
            if (item == null || item.target == null) continue;
            CanvasGroup group = item.target.GetComponent<CanvasGroup>();
            if (group == null) group = item.target.AddComponent<CanvasGroup>();
            if (!result.Contains(group)) result.Add(group);
        }
        return result;
    }

    private void StartFirstSpreadArrangement()
    {
        if (introController != null)
            introController.RestoreFirstSpreadDewPosition();

        foreach (RevisitArrangementItem item in firstSpreadArrangement)
        {
            if (item == null || item.target == null) continue;
            CanvasGroup group = item.target.GetComponent<CanvasGroup>();
            if (group == null) group = item.target.AddComponent<CanvasGroup>();
            group.alpha = 0f;
        }
        foreach (RevisitArrangementItem item in firstSpreadArrangement)
        {
            if (item == null || item.target == null) continue;
            activeAnimations.Add(StartCoroutine(AnimateArrangementItem(item)));
        }
    }

    private IEnumerator AnimateArrangementItem(RevisitArrangementItem item)
    {
        if (item.delay > 0f)
            yield return new WaitForSecondsRealtime(item.delay);
        if (item.target == null) yield break;
        CanvasGroup group = item.target.GetComponent<CanvasGroup>();
        if (group == null) yield break;
        float duration = Mathf.Max(0f, item.duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed = Mathf.Min(duration, elapsed + Time.unscaledDeltaTime);
            group.alpha = Mathf.Lerp(0f, item.targetAlpha,
                duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }
        group.alpha = item.targetAlpha;
    }

    // Reuse the exact existing fade/color/sequence animation engine for prefab pages.
    public void PlayPrefabArtwork(BookPagePrefab prefabPage)
    {
        StopActiveAnimations();
        if (prefabPage == null || prefabPage.artwork == null) return;
        foreach (ArtworkEntry entry in prefabPage.artwork)
        {
            if (entry == null) continue;
            ApplyStartingAppearance(entry);
            if (entry.animation != ArtworkAnimation.None)
                activeAnimations.Add(StartCoroutine(AnimateArtwork(entry)));
        }
    }

    public void StopPrefabArtwork()
    {
        StopActiveAnimations();
    }

    private void ActivateSpread(int index)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] == null || pages[i].pageObject == null)
                continue;

            pages[i].pageObject.SetActive(i == index);
        }

        Log("Showing spread " + (index + 1) +
            " / " + pages.Count);
    }

    private void InitializePageArtwork(BookPage page)
    {
        if (page == null || page.artwork == null)
            return;

        foreach (ArtworkEntry entry in page.artwork)
        {
            if (entry == null) continue;
            CacheOriginalTransform(entry);
            ApplyStartingAppearance(entry);
        }
        HideAllInteractionPrompts(page);
    }

    private static bool HasArtworkTarget(ArtworkEntry entry)
    {
        return entry != null &&
               (entry.graphic != null || entry.canvasGroup != null);
    }

    private void ApplyStartingAppearance(ArtworkEntry entry)
    {
        if (entry == null)
            return;

        // Keep every original Graphic setting and behavior unchanged.
        if (entry.graphic == null)
        {
            if (entry.canvasGroup != null &&
                (entry.animation == ArtworkAnimation.Fade ||
                 entry.animation == ArtworkAnimation.Sequence))
                entry.canvasGroup.alpha = entry.startingAlpha;

            return;
        }

        Color color = entry.graphic.color;

        switch (entry.animation)
        {
            case ArtworkAnimation.None:
                break;

            case ArtworkAnimation.Fade:
                color.a = entry.startingAlpha;
                entry.graphic.color = color;
                break;

            case ArtworkAnimation.ColorShifts:
                entry.graphic.color = entry.startingColor;
                break;

            case ArtworkAnimation.Sequence:
                entry.graphic.color = entry.startingColor;
                break;
        }
    }

    private void StartSpreadArtwork(int index)
    {
        StopActiveAnimations();
        if (index < 0 || index >= pages.Count) return;
        BookPage page = pages[index];
        if (page == null) return;
        if (page.artwork != null)
            foreach (ArtworkEntry entry in page.artwork)
            {
                if (entry == null) continue;
                CacheOriginalTransform(entry);
                RestoreOriginalTransform(entry);
                ApplyStartingAppearance(entry);
            }
        HideAllInteractionPrompts(page);
        int version = artworkRunVersion;
        bool revisiting = previouslyCompletedSpreads.Contains(index);
        bool playKeys = page.interactKeys != null && page.interactKeys.Count > 0 &&
                        (!revisiting || page.replayInteractionsOnRevisit);
        interactionsFinished = !playKeys;
        if (!playKeys && page.interactKeys != null)
            foreach (InteractKey key in page.interactKeys)
                if (key != null && !string.IsNullOrWhiteSpace(key.name))
                    firedInteractions.Add(key.name.Trim());
        ValidateSpread(page);
        if (page.artwork != null)
            foreach (ArtworkEntry entry in page.artwork)
                if (entry != null)
                    activeAnimations.Add(StartCoroutine(RunArtwork(page, entry, version)));
        if (playKeys)
            activeAnimations.Add(StartCoroutine(RunInteractions(page, index, version)));
    }

    private bool CanLeaveCurrentSpread()
    {
        if (pages == null || currentSpreadIndex < 0 || currentSpreadIndex >= pages.Count)
            return true;
        BookPage page = pages[currentSpreadIndex];
        if (page == null) return true;
        if (page.requireInteractionsBeforeTurning && !interactionsFinished) return false;
        if (!page.waitForArtworkBeforeTurning) return true;
        ArtworkEntry unlock = FindArtwork(page, page.unlockArtworkName);
        if (unlock == null)
        {
            WarnOnce("Navigation unlock artwork not found on '" + page.name + "'.");
            return false;
        }
        if (!HasEvent(unlock, page.unlockEvent)) return false;
        if (pageTurnUnlockTime < 0f)
            pageTurnUnlockTime = Time.unscaledTime + Mathf.Max(0f, page.unlockDelay);
        return Time.unscaledTime >= pageTurnUnlockTime;
    }

    private IEnumerator RunInteractions(BookPage page, int index, int version)
    {
        foreach (InteractKey key in page.interactKeys)
        {
            if (key == null || string.IsNullOrWhiteSpace(key.name)) continue;
            string id = key.name.Trim();
            if (key.promptTiming == PromptTiming.AfterArtwork &&
                !string.IsNullOrWhiteSpace(key.afterArtwork))
            {
                ArtworkEntry predecessor = FindArtwork(page, key.afterArtwork);
                if (predecessor == null)
                {
                    WarnOnce("Unknown prompt dependency: " + key.afterArtwork);
                    continue;
                }
                while (version == artworkRunVersion &&
                       !HasEvent(predecessor, key.afterEvent))
                    yield return null;
            }
            if (version != artworkRunVersion) yield break;
            if (key.promptDelay > 0f)
                yield return new WaitForSecondsRealtime(key.promptDelay);
            if (version != artworkRunVersion) yield break;
            yield return FadePrompt(key, 1f, key.promptFadeIn);
            if (version != artworkRunVersion) yield break;
            // One press fires exactly one named interaction, in list order.
            while (version == artworkRunVersion && !Input.GetKeyDown(key.key))
                yield return null;
            if (version != artworkRunVersion) yield break;
            firedInteractions.Add(id);
            if (key.pressSFX != null)
                PlayOneShot(key.pressSFX, key.pressVolume);
            yield return FadePrompt(key, 0f, key.promptFadeOut);
            yield return null; // prevent one held frame from firing the next E stage
        }
        if (version == artworkRunVersion)
        {
            interactionsFinished = true;
            previouslyCompletedSpreads.Add(index);
        }
    }

    private IEnumerator FadePrompt(InteractKey key, float to, float duration)
    {
        if (key.promptGraphic == null && key.promptCanvasGroup == null) yield break;
        float from = key.promptCanvasGroup != null
            ? key.promptCanvasGroup.alpha : key.promptGraphic.color.a;
        float elapsed = 0f;
        float length = Mathf.Max(0f, duration);
        do
        {
            elapsed = Mathf.Min(length, elapsed + Time.unscaledDeltaTime);
            float t = length <= 0f ? 1f : elapsed / length;
            float alpha = Mathf.Lerp(from, to, t);
            if (key.promptCanvasGroup != null)
                key.promptCanvasGroup.alpha = alpha;
            else if (key.promptGraphic != null)
            {
                Color c = key.promptGraphic.color;
                c.a = alpha;
                key.promptGraphic.color = c;
            }
            if (t >= 1f) break;
            yield return null;
        } while (true);
    }

    private void HideAllInteractionPrompts(BookPage page)
    {
        if (page == null || page.interactKeys == null) return;
        foreach (InteractKey key in page.interactKeys)
        {
            if (key == null) continue;
            if (key.promptCanvasGroup != null) key.promptCanvasGroup.alpha = 0f;
            else if (key.promptGraphic != null)
            {
                Color c = key.promptGraphic.color;
                c.a = 0f;
                key.promptGraphic.color = c;
            }
        }
    }

    private ArtworkEntry FindArtwork(BookPage page, string name)
    {
        if (page == null || page.artwork == null || string.IsNullOrWhiteSpace(name))
            return null;
        foreach (ArtworkEntry entry in page.artwork)
            if (entry != null && string.Equals(entry.name?.Trim(), name.Trim(),
                    StringComparison.Ordinal)) return entry;
        return null;
    }

    private bool HasEvent(ArtworkEntry entry, ArtworkEvent type)
    {
        switch (type)
        {
            case ArtworkEvent.Started: return startedArtwork.Contains(entry);
            case ArtworkEvent.Completed: return completedArtwork.Contains(entry);
            case ArtworkEvent.SpecialStarted: return startedSpecials.Contains(entry);
            case ArtworkEvent.SpecialCompleted: return completedSpecials.Contains(entry);
        }
        return false;
    }

    private IEnumerator RunArtwork(BookPage page, ArtworkEntry entry, int version)
    {
        if (entry.waitForInteractKey)
        {
            if (string.IsNullOrWhiteSpace(entry.interactionName) ||
                page.interactKeys == null ||
                !page.interactKeys.Exists(x => x != null && !string.IsNullOrWhiteSpace(x.name) &&
                     x.name.Trim() == entry.interactionName.Trim()))
            {
                WarnOnce("No interaction defined for artwork '" + entry.name + "'.");
                yield break;
            }
            while (version == artworkRunVersion &&
                   !firedInteractions.Contains(entry.interactionName.Trim()))
                yield return null;
        }
        if (entry.dependencies != null)
            foreach (ArtworkDependency dependency in entry.dependencies)
            {
                if (dependency == null || string.IsNullOrWhiteSpace(dependency.artworkName))
                    continue;
                ArtworkEntry target = FindArtwork(page, dependency.artworkName);
                if (target == null || target == entry)
                {
                    WarnOnce("Invalid dependency '" + dependency.artworkName +
                             "' for artwork '" + entry.name + "'.");
                    yield break;
                }
                while (version == artworkRunVersion && !HasEvent(target, dependency.waitFor))
                    yield return null;
            }
        if (version != artworkRunVersion) yield break;
        startedArtwork.Add(entry);
        yield return AnimateArtwork(entry);
        if (version != artworkRunVersion) yield break;
        completedArtwork.Add(entry);
        if (entry.specialAnimation != SpecialAnimation.None)
            activeAnimations.Add(StartCoroutine(RunSpecial(page, entry, version)));
    }

    private IEnumerator RunSpecial(BookPage page, ArtworkEntry entry, int version)
    {
        if (entry.specialDependencies != null)
            foreach (ArtworkDependency dependency in entry.specialDependencies)
            {
                if (dependency == null || string.IsNullOrWhiteSpace(dependency.artworkName))
                    continue;
                ArtworkEntry target = FindArtwork(page, dependency.artworkName);
                if (target == null || target == entry)
                {
                    WarnOnce("Invalid special dependency '" + dependency.artworkName +
                             "' for artwork '" + entry.name + "'.");
                    yield break;
                }
                while (version == artworkRunVersion && !HasEvent(target, dependency.waitFor))
                    yield return null;
            }
        if (version != artworkRunVersion) yield break;
        if (entry.specialAnimation == SpecialAnimation.Dew)
            yield return AnimateDew(entry, version);
        else if (entry.specialAnimation == SpecialAnimation.Steam)
            yield return AnimateSteam(entry, version);
    }

    private void ValidateSpread(BookPage page)
    {
        HashSet<string> names = new HashSet<string>();
        if (page.artwork != null)
            foreach (ArtworkEntry e in page.artwork)
                if (e != null && !string.IsNullOrWhiteSpace(e.name) &&
                    !names.Add(e.name.Trim()))
                    WarnOnce("Duplicate artwork name '" + e.name + "' on spread '" + page.name + "'.");
        names.Clear();
        if (page.interactKeys != null)
            foreach (InteractKey key in page.interactKeys)
            {
                if (key == null || string.IsNullOrWhiteSpace(key.name)) continue;
                if (!names.Add(key.name.Trim()))
                    WarnOnce("Duplicate interaction name '" + key.name + "' on spread '" + page.name + "'.");
                if (key.promptTiming == PromptTiming.AfterArtwork)
                {
                    ArtworkEntry dependency = FindArtwork(page, key.afterArtwork);
                    if (dependency == null)
                        WarnOnce("Prompt for '" + key.name + "' has no valid After Artwork.");
                    else if (dependency.waitForInteractKey &&
                             dependency.interactionName == key.name)
                        WarnOnce("Prompt for '" + key.name +
                                 "' waits for artwork triggered by itself. This will deadlock.");
                }
            }
    }

    private void WarnOnce(string message)
    {
        if (reportedConfigurationIssues.Add(message))
            Debug.LogWarning("[IntroBook] " + message, this);
    }

    private IEnumerator AnimateArtwork(ArtworkEntry entry)
    {
        if (entry == null) yield break;
        initialPlayed[entry] = false;
        ambientPlayed[entry] = false;
        if (entry.animation == ArtworkAnimation.Fade)
        {
            if (entry.delay > 0f)
                yield return new WaitForSecondsRealtime(entry.delay);
            float from = GetTargetAlpha(entry);
            if (entry.graphic != null)
                yield return FadeGraphic(entry.graphic, entry.targetAlpha,
                    entry.duration, a => SoundThreshold(entry, a, entry.targetAlpha >= from));
            else if (entry.canvasGroup != null)
                yield return FadeCanvasGroup(entry.canvasGroup, entry.targetAlpha,
                    entry.duration, a => SoundThreshold(entry, a, entry.targetAlpha >= from));
            else
                SoundThreshold(entry, 1f, true);
        }
        else if (entry.animation == ArtworkAnimation.ColorShifts)
        {
            if (entry.graphic == null || entry.shifts == null) yield break;
            foreach (ColorShift shift in entry.shifts)
            {
                if (shift == null || entry.graphic == null) continue;
                if (shift.delay > 0f)
                    yield return new WaitForSecondsRealtime(shift.delay);
                if (entry.graphic == null) yield break;
                yield return ShiftGraphicColor(entry.graphic, shift.targetColor,
                    shift.duration, a => SoundThreshold(entry, a, true));
            }
        }
        else if (entry.animation == ArtworkAnimation.Sequence)
        {
            if (entry.steps == null) yield break;
            foreach (AnimationStep step in entry.steps)
            {
                if (step == null || !HasArtworkTarget(entry)) continue;
                if (step.delay > 0f)
                    yield return new WaitForSecondsRealtime(step.delay);
                if (!HasArtworkTarget(entry)) yield break;
                if (step.type == AnimationStepType.Fade)
                {
                    float from = GetTargetAlpha(entry);
                    if (entry.graphic != null)
                        yield return FadeGraphic(entry.graphic, step.targetAlpha,
                            step.duration, a => SoundThreshold(entry, a, step.targetAlpha >= from));
                    else
                        yield return FadeCanvasGroup(entry.canvasGroup, step.targetAlpha,
                            step.duration, a => SoundThreshold(entry, a, step.targetAlpha >= from));
                }
                else if (step.type == AnimationStepType.ColorShift && entry.graphic != null)
                    yield return ShiftGraphicRGB(entry.graphic, step.targetColor,
                        step.duration, a => SoundThreshold(entry, a, true));
            }
        }
        else
            SoundThreshold(entry, HasArtworkTarget(entry) ? GetTargetAlpha(entry) : 1f, true);
    }

    private float GetTargetAlpha(ArtworkEntry entry)
    {
        if (entry.graphic != null) return entry.graphic.color.a;
        if (entry.canvasGroup != null) return entry.canvasGroup.alpha;
        return 1f;
    }

    private void SetTargetAlpha(ArtworkEntry entry, float alpha)
    {
        if (entry.graphic != null)
        {
            Color c = entry.graphic.color;
            c.a = alpha;
            entry.graphic.color = c;
        }
        else if (entry.canvasGroup != null)
            entry.canvasGroup.alpha = alpha;
    }

    private void SoundThreshold(ArtworkEntry entry, float alpha, bool entering)
    {
        if (entry == null) return;
        if (!entering)
        {
            if (artworkSounds.TryGetValue(entry, out List<AudioSource> sources))
                foreach (AudioSource source in sources)
                    if (source != null)
                        source.volume = (source.loop ? entry.ambientVolume : entry.initialVolume) *
                                        Mathf.Clamp01(alpha);
            if (alpha <= 0f) StopEntrySounds(entry);
            return;
        }
        bool once = initialPlayed.ContainsKey(entry) && initialPlayed[entry];
        bool loop = ambientPlayed.ContainsKey(entry) && ambientPlayed[entry];
        if (!once && entry.initialSFX != null && alpha >= entry.initialSFXTriggerAlpha)
        {
            PlayEntrySound(entry, entry.initialSFX, entry.initialVolume, false);
            initialPlayed[entry] = true;
        }
        if (!loop && entry.ambientSFX != null && alpha >= entry.ambientStartAlpha)
        {
            PlayEntrySound(entry, entry.ambientSFX, entry.ambientVolume, true);
            ambientPlayed[entry] = true;
        }
    }

    private void PlayEntrySound(ArtworkEntry entry, AudioClip clip, float volume, bool loop)
    {
        if (clip == null) return;
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.Play();
        if (!artworkSounds.ContainsKey(entry))
            artworkSounds[entry] = new List<AudioSource>();
        artworkSounds[entry].Add(source);
        if (!loop) Destroy(source, clip.length + 0.1f);
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null) return;
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.PlayOneShot(clip, volume);
        Destroy(source, clip.length + 0.1f);
    }

    private void StopEntrySounds(ArtworkEntry entry)
    {
        if (!artworkSounds.TryGetValue(entry, out List<AudioSource> sources)) return;
        foreach (AudioSource source in sources)
            if (source != null) { source.Stop(); Destroy(source); }
        sources.Clear();
    }

    private void StopAllArtworkSounds()
    {
        foreach (var pair in artworkSounds)
            foreach (AudioSource source in pair.Value)
                if (source != null) { source.Stop(); Destroy(source); }
        artworkSounds.Clear();
    }

    private void CacheOriginalTransform(ArtworkEntry entry)
    {
        if (entry == null || entry.specialAnimation != SpecialAnimation.Dew ||
            !HasArtworkTarget(entry) || originalScales.ContainsKey(entry)) return;
        Transform t = entry.graphic != null ? entry.graphic.transform : entry.canvasGroup.transform;
        originalScales[entry] = t.localScale;
        originalPositions[entry] = t.localPosition;
        if (t is RectTransform rect) originalAnchors[entry] = rect.anchoredPosition;
    }

    private void RestoreOriginalTransform(ArtworkEntry entry)
    {
        if (entry == null || !originalScales.ContainsKey(entry) || !HasArtworkTarget(entry)) return;
        Transform t = entry.graphic != null ? entry.graphic.transform : entry.canvasGroup.transform;
        t.localScale = originalScales[entry];
        if (t is RectTransform rect && originalAnchors.TryGetValue(entry, out Vector2 a))
            rect.anchoredPosition = a;
        else t.localPosition = originalPositions[entry];
    }

    private IEnumerator AnimateDew(ArtworkEntry entry, int version)
    {
        if (!HasArtworkTarget(entry)) yield break;
        if (entry.dewExitDelay > 0f)
            yield return new WaitForSecondsRealtime(entry.dewExitDelay);
        if (version != artworkRunVersion) yield break;
        CacheOriginalTransform(entry);
        startedSpecials.Add(entry); // dependencies can launch Steam here
        Transform t = entry.graphic != null ? entry.graphic.transform : entry.canvasGroup.transform;
        RectTransform rect = t as RectTransform;
        float fromAlpha = GetTargetAlpha(entry);
        Vector3 fromScale = t.localScale;
        Vector3 endScale = originalScales[entry] * entry.dewEndScale;
        Vector3 fromPosition = t.localPosition;
        Vector3 endPosition = originalPositions[entry] + Vector3.down * entry.dewDropDistance;
        Vector2 fromAnchor = rect != null ? rect.anchoredPosition : Vector2.zero;
        Vector2 endAnchor = rect != null ? originalAnchors[entry] + Vector2.down * entry.dewDropDistance : Vector2.zero;
        float length = Mathf.Max(0f, entry.dewExitDuration);
        float elapsed = 0f;
        while (version == artworkRunVersion)
        {
            elapsed = Mathf.Min(length, elapsed + Time.unscaledDeltaTime);
            float progress = length <= 0f ? 1f : elapsed / length;
            SetTargetAlpha(entry, Mathf.Lerp(fromAlpha, 0f, progress));
            t.localScale = Vector3.Lerp(fromScale, endScale, progress);
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(fromAnchor, endAnchor, progress);
            else
                t.localPosition = Vector3.Lerp(fromPosition, endPosition, progress);
            if (progress >= 1f) break;
            yield return null;
        }
        if (version == artworkRunVersion)
        {
            completedSpecials.Add(entry);
            StopEntrySounds(entry);
        }
    }

    private IEnumerator AnimateSteam(ArtworkEntry entry, int version)
    {
        Graphic graphic = entry.steamGraphic != null ? entry.steamGraphic : entry.graphic;
        if (graphic == null && entry.canvasGroup != null)
            graphic = entry.canvasGroup.GetComponentInChildren<Graphic>(true);
        if (graphic == null)
        {
            WarnOnce("Steam requires a Graphic for '" + entry.name + "'.");
            yield break;
        }
        startedSpecials.Add(entry);
        while (version == artworkRunVersion && graphic != null)
        {
            float cycle = Mathf.Max(0.01f, entry.steamCycleDuration);
            float wave = 0.5f + 0.5f * Mathf.Sin(
                (Time.unscaledTime / cycle + entry.steamPhaseOffset) * Mathf.PI * 2f);
            Color c = graphic.color;
            c.a = Mathf.Lerp(entry.steamMinimumAlpha, entry.steamMaximumAlpha, wave);
            graphic.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup group, float targetAlpha, float duration,
        Action<float> onAlpha = null)
    {
        if (group == null)
            yield break;

        float from = group.alpha;
        float to = Mathf.Clamp01(targetAlpha);
        float length = Mathf.Max(0f, duration);

        if (length <= 0f)
        {
            group.alpha = to;
            onAlpha?.Invoke(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < length)
        {
            if (group == null)
                yield break;

            elapsed = Mathf.Min(length, elapsed + Time.unscaledDeltaTime);
            group.alpha = Mathf.Lerp(from, to, elapsed / length);
            onAlpha?.Invoke(group.alpha);
            yield return null;
        }

        if (group != null)
        {
            group.alpha = to;
            onAlpha?.Invoke(to);
        }
    }

    private IEnumerator FadeGraphic(
        Graphic graphic, float targetAlpha, float duration,
        Action<float> onAlpha = null)
    {
        if (graphic == null)
            yield break;

        Color target = graphic.color;
        target.a = Mathf.Clamp01(targetAlpha);

        yield return ShiftGraphicColor(
            graphic, target, duration, onAlpha);
    }

    private IEnumerator ShiftGraphicRGB(
        Graphic graphic, Color targetColor, float duration,
        Action<float> onAlpha = null)
    {
        if (graphic == null)
            yield break;

        Color target = targetColor;
        target.a = graphic.color.a;

        yield return ShiftGraphicColor(
            graphic, target, duration, onAlpha);
    }

    private IEnumerator ShiftGraphicColor(
        Graphic graphic, Color targetColor, float duration,
        Action<float> onAlpha = null)
    {
        if (graphic == null)
            yield break;

        Color from = graphic.color;
        float length = Mathf.Max(0f, duration);

        if (length <= 0f)
        {
            graphic.color = targetColor;
            onAlpha?.Invoke(targetColor.a);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < length)
        {
            if (graphic == null)
                yield break;

            elapsed = Mathf.Min(
                length, elapsed + Time.unscaledDeltaTime);

            graphic.color = Color.Lerp(
                from, targetColor, elapsed / length);
            onAlpha?.Invoke(graphic.color.a);
            yield return null;
        }

        if (graphic != null)
        {
            graphic.color = targetColor;
            onAlpha?.Invoke(targetColor.a);
        }
    }

    private List<CanvasGroup> GetPageCanvasGroups(BookPage page)
    {
        List<CanvasGroup> result = new List<CanvasGroup>();

        if (page == null || page.artwork == null)
            return result;

        foreach (ArtworkEntry entry in page.artwork)
        {
            if (entry == null || entry.graphic != null ||
                entry.canvasGroup == null)
                continue;

            if (!result.Contains(entry.canvasGroup))
                result.Add(entry.canvasGroup);
        }

        return result;
    }

    private List<Graphic> GetPageGraphics(BookPage page)
    {
        List<Graphic> result = new List<Graphic>();

        if (page == null || page.artwork == null)
            return result;

        foreach (ArtworkEntry entry in page.artwork)
        {
            if (entry == null || entry.graphic == null)
                continue;

            if (!result.Contains(entry.graphic))
                result.Add(entry.graphic);
        }

        return result;
    }

    private void StopActiveAnimations()
    {
        artworkRunVersion++;
        startedArtwork.Clear();
        completedArtwork.Clear();
        startedSpecials.Clear();
        completedSpecials.Clear();
        firedInteractions.Clear();
        interactionsFinished = true;
        pageTurnUnlockTime = -1f;
        StopAllArtworkSounds();
        foreach (Coroutine routine in activeAnimations)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        activeAnimations.Clear();
    }

    private void PlayPageTurnSFX()
    {
        if (pageTurnSFXSource != null &&
            pageTurnSFXClip != null)
        {
            pageTurnSFXSource.PlayOneShot(
                pageTurnSFXClip, pageTurnSFXVolume);
        }
    }

    public bool IsOnLastSpread()
    {
        return pages != null &&
               pages.Count > 0 &&
               currentSpreadIndex == pages.Count - 1;
    }

    public void Continue()
    {
        if (isEnding || isTurningPage)
            return;

        if (pages == null || pages.Count == 0)
            return;

        if (requireLastSpreadForConfirm && !IsOnLastSpread())
            return;

        if (!CanLeaveCurrentSpread())
            return;

        if (string.IsNullOrWhiteSpace(nextLevelName))
        {
            Debug.LogError(
                "[IntroBook] Next Level Name is empty.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextLevelName))
        {
            Debug.LogError(
                "[IntroBook] Scene not in build or not loadable: " +
                nextLevelName, this);
            return;
        }

        StopActiveAnimations();

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
                length, elapsed + Time.unscaledDeltaTime);

            ApplyFade(length <= 0f
                ? 0f
                : 1f - elapsed / length);

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

            if (!originalVolumes.TryGetValue(
                    source, out float original))
            {
                original = source.volume;
            }

            source.volume = original * alpha;
        }
    }

    private void HandoffToLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError(
                "[IntroBook] LevelManager.Instance is missing.",
                this);
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
                levelManagerSave);
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
