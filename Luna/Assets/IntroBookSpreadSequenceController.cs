
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
    private enum RibbonMode { None, Special, PageTurn, CloseBook }

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

        [Header("Optional Persistence")]
        [Tooltip("Keep this target visible for the rest of this book-opening session. " +
                 "Place it outside individual Page GameObjects.")]
        public bool keepVisibleForBookSession = false;
        [Tooltip("When both consecutive spreads assign the SAME target and enable this, " +
                 "it is never faded or reanimated between those two spreads.")]
        public bool keepVisibleWhenShared = false;
        [Tooltip("On revisiting this spread, restore the artwork's completed appearance " +
                 "instead of replaying its animation.")]
        public bool keepFinalStateOnRevisit = false;

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

        [Header("Optional Page Unlocking")]
        [Tooltip("Unique identifier used by BookPageUnlocker. May be the spread name, but keep it stable once published.")]
        public string pageId = "";
        [Tooltip("Enabled by default to preserve existing page visibility.")]
        public bool availableFromStart = true;
        [Tooltip("The opening with the closed book. Not included in backwards page navigation.")]
        public bool isOpeningSpread = false;
        [Tooltip("Automatically turn to the next available spread after this spread's interactions.")]
        public bool autoAdvanceAfterInteractions = false;
        [Tooltip("Optional artwork to finish before automatic advance (for example Open Book).")]
        public string autoAdvanceAfterArtwork = "";
        public ArtworkEvent autoAdvanceAfterEvent = ArtworkEvent.Completed;
        [Min(0f)] public float autoAdvanceDelay = 0f;
        [Tooltip("Stop page turning and Continue until this spread's named interactions have been pressed.")]
        public bool requireInteractionsBeforeTurning = true;
        [Tooltip("Normally completed interactions are skipped when a spread is revisited.")]
        public bool replayInteractionsOnRevisit = false;
        [Tooltip("Optionally keep navigation locked until one artwork event is reached.")]
        public bool waitForArtworkBeforeTurning = false;
        public string unlockArtworkName = "";
        public ArtworkEvent unlockEvent = ArtworkEvent.Completed;
        [Min(0f)] public float unlockDelay = 0f;

        [Header("Optional Ribbon Group")]
        [Tooltip("Parent for this spread's ribbons, outside all Page GameObjects. Add a CanvasGroup to fade the parent; one will be added at runtime if missing.")]
        public GameObject ribbonGroup;
        [Tooltip("Child for the one-time special action, such as Gain Wand / W.")]
        public GameObject specialRibbon;
        [Tooltip("Child for the usual A/D page-turn controls.")]
        public GameObject pageTurnRibbon;
        [Tooltip("Child for Close Book / E, displayed on the last available spread.")]
        public GameObject closeBookRibbon;
        [Tooltip("When enabled, the special ribbon appears until its action is completed once. Afterwards page-turn or close-book ribbons take over.")]
        public bool useOneTimeSpecialRibbon = false;
        [Tooltip("Persistent unique ID for the one-time action, e.g. GainWand. Do not change this once saves exist.")]
        public string specialRibbonId = "";
        public KeyCode specialRibbonKey = KeyCode.W;
        [Tooltip("ON: pressing the special key leaves the book using the existing Continue transition, even if this isn't the last spread.")]
        public bool specialRibbonExitsBook = true;
        [Tooltip("Optional action callback, such as granting the player's wand ability. Saving the ribbon completion alone does not grant inventory items.")]
        public UnityEvent onSpecialRibbonActivated = new UnityEvent();
        [Tooltip("Optional artwork whose event must happen before any ribbon on this spread appears.")]
        public string ribbonReadyArtwork = "";
        public ArtworkEvent ribbonReadyEvent = ArtworkEvent.Completed;
        [Min(0f)] public float ribbonFadeDuration = 0.25f;
    }

    [Header("Book Pages")]
    [SerializeField]
    private List<BookPage> pages = new List<BookPage>();

    [Header("Previously Visited Pages")]
    [SerializeField] private IntroBookVisitedPagesController visitedPagesController;

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
    [Tooltip("When a spread uses a ribbon group, E closes the book only when its Close Book ribbon is active.")]
    [SerializeField] private KeyCode closeBookKey = KeyCode.E;

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
    private readonly HashSet<UnityEngine.Object> sessionVisibleTargets =
        new HashSet<UnityEngine.Object>();

    private sealed class StoredAppearance
    {
        public bool hasGraphic;
        public Color graphicColor;
        public bool hasGroup;
        public float groupAlpha;
        public Vector3 localScale;
        public Vector3 localPosition;
        public Vector2 anchoredPosition;
        public bool isRect;
    }
    private readonly Dictionary<ArtworkEntry, StoredAppearance> completedAppearances =
        new Dictionary<ArtworkEntry, StoredAppearance>();
    private bool interactionsFinished = true;
    private int artworkRunVersion = 0;
    private float pageTurnUnlockTime = -1f;

    private Coroutine ribbonRoutine;
    private int shownRibbonIndex = -1;
    private RibbonMode shownRibbonMode = RibbonMode.None;
    private int requestedRibbonIndex = -1;
    private RibbonMode requestedRibbonMode = RibbonMode.None;
    private bool ribbonTransitioning;

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
        StopRibbonTransition();
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
        if (!IsPageAvailable(currentSpreadIndex))
            currentSpreadIndex = FindAvailablePage(-1, 1);
        if (currentSpreadIndex < 0)
        {
            Debug.LogWarning("[IntroBook] No available spreads.", this);
            return;
        }

        for (int i = 0; i < pages.Count; i++)
            InitializePageArtwork(pages[i]);
        InitializeRibbonGroups();

        ActivateSpread(currentSpreadIndex);
        StartSpreadArtwork(currentSpreadIndex);
        RecordCurrentPage();
    }

    private void Update()
    {
        if (isEnding || viewingHistory)
            return;

        RefreshRibbon();

        if (isTurningPage) return;

        BookPage page = CurrentBookPage();
        if (page != null && page.ribbonGroup != null)
        {
            // A configured ribbon owns its displayed key. Do not also process
            // the old global W/Continue input on these spreads.
            if (!ribbonTransitioning && shownRibbonIndex == currentSpreadIndex)
            {
                if (shownRibbonMode == RibbonMode.Special &&
                    Input.GetKeyDown(page.specialRibbonKey))
                {
                    ActivateSpecialRibbon(page);
                    return;
                }
                if (shownRibbonMode == RibbonMode.CloseBook &&
                    Input.GetKeyDown(closeBookKey))
                {
                    TryContinue(false, false);
                    return;
                }
            }
        }
        else if (Input.GetKeyDown(confirmKey))
        {
            // Existing non-ribbon spreads retain W/Continue exactly as before.
            Continue();
        }

        if (Time.unscaledTime >= nextAllowedPageTurnTime &&
            CanLeaveCurrentSpread())
        {
            if (Input.GetKeyDown(nextPageKey1) ||
                Input.GetKeyDown(nextPageKey2))
                NextSpread();
            else if (Input.GetKeyDown(previousPageKey1) ||
                     Input.GetKeyDown(previousPageKey2))
                PreviousSpread();
        }
    }

    public void NextSpread()
    {
        int next = FindAvailablePage(currentSpreadIndex, 1);
        if (next >= 0) ShowSpread(next);
    }

    public void PreviousSpread()
    {
        if (!CanLeaveCurrentSpread()) return;
        int previous = FindAvailablePage(currentSpreadIndex, -1);
        if (previous >= 0)
        {
            ShowSpread(previous);
            return;
        }
        // Preserve old history behavior if this book has no separate opening spread.
        if (!HasOpeningSpread() && currentSpreadIndex == 0 &&
            visitedPagesController != null)
            visitedPagesController.EnterHistory();
    }

    public void ShowSpread(int index)
    {
        if (isEnding || isTurningPage || viewingHistory ||
            pages == null || pages.Count == 0)
            return;

        if (Time.unscaledTime < nextAllowedPageTurnTime)
            return;

        if (index < 0 || index >= pages.Count || !IsPageAvailable(index) ||
            (HasOpeningSpread() && pages[index].isOpeningSpread &&
             currentSpreadIndex != index) ||
            index == currentSpreadIndex || !CanLeaveCurrentSpread())
            return;

        StartCoroutine(TurnPage(index));
    }

    private IEnumerator TurnPage(int targetIndex)
    {
        isTurningPage = true;
        StartRibbonFadeOut(pageTurnFadeDuration);
        StopActiveAnimations();
        PlayPageTurnSFX();

        BookPage outgoingPage = pages[currentSpreadIndex];
        List<Graphic> outgoing = GetPageGraphics(outgoingPage);
        outgoing.RemoveAll(graphic => RetainTargetOnTransition(
            outgoingPage, pages[targetIndex], graphic));
        List<CanvasGroup> outgoingGroups = GetPageCanvasGroups(outgoingPage);
        outgoingGroups.RemoveAll(group => RetainTargetOnTransition(
            outgoingPage, pages[targetIndex], group));
        List<float> outgoingGroupStarts = new List<float>();
        foreach (CanvasGroup group in outgoingGroups)
            outgoingGroupStarts.Add(group.alpha);

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


            yield return null;
        }

        foreach (CanvasGroup group in outgoingGroups)
            if (group != null) group.alpha = 0f;


        foreach (Graphic graphic in outgoing)
        {
            if (graphic == null)
                continue;

            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
        }

        // The ribbon fade runs alongside the artwork page fade.
        HideAllRibbonGroups();
        int previousIndex = currentSpreadIndex;
        currentSpreadIndex = targetIndex;

        ActivateSpread(currentSpreadIndex);
        StartSpreadArtwork(currentSpreadIndex, previousIndex);
        RecordCurrentPage();

        nextAllowedPageTurnTime =
            Time.unscaledTime + Mathf.Max(0f, pageTurnCooldown);

        isTurningPage = false;
    }

    // Ribbon UI is deliberately independent of spread GameObjects/artwork.
    // Each spread chooses a parent group (left or right), and its three child
    // variants are switched without rebuilding or cloning those scene objects.
    private BookPage CurrentBookPage()
    {
        return pages != null && currentSpreadIndex >= 0 &&
               currentSpreadIndex < pages.Count ? pages[currentSpreadIndex] : null;
    }

    private CanvasGroup RibbonCanvas(BookPage page)
    {
        if (page == null || page.ribbonGroup == null) return null;
        CanvasGroup group = page.ribbonGroup.GetComponent<CanvasGroup>();
        return group != null ? group : page.ribbonGroup.AddComponent<CanvasGroup>();
    }

    private void InitializeRibbonGroups()
    {
        if (pages == null) return;
        HashSet<GameObject> initialized = new HashSet<GameObject>();
        foreach (BookPage page in pages)
        {
            if (page == null || page.ribbonGroup == null) continue;
            // Multiple spreads can reference the same left/right parent but
            // have different child variants. Initialize every assigned child.
            if (page.specialRibbon != null) page.specialRibbon.SetActive(false);
            if (page.pageTurnRibbon != null) page.pageTurnRibbon.SetActive(false);
            if (page.closeBookRibbon != null) page.closeBookRibbon.SetActive(false);
            if (initialized.Add(page.ribbonGroup))
            {
                RibbonCanvas(page);
                page.ribbonGroup.SetActive(false);
            }
        }
        shownRibbonIndex = -1;
        shownRibbonMode = RibbonMode.None;
        requestedRibbonIndex = -1;
        requestedRibbonMode = RibbonMode.None;
    }

    private string SpecialRibbonId(BookPage page)
    {
        if (page == null) return "";
        if (!string.IsNullOrWhiteSpace(page.specialRibbonId)) return page.specialRibbonId.Trim();
        if (!string.IsNullOrWhiteSpace(page.pageId)) return "Ribbon." + page.pageId.Trim();
        return "Ribbon." + page.name.Trim();
    }

    private bool IsSpecialRibbonComplete(BookPage page)
    {
        return page == null || !page.useOneTimeSpecialRibbon ||
               FlorilegiumProgress.IsRibbonActionCompleted(SpecialRibbonId(page));
    }

    private bool IsRibbonReady(BookPage page)
    {
        if (page == null || !interactionsFinished ||
            !CanLeaveCurrentSpread(true)) return false;
        if (string.IsNullOrWhiteSpace(page.ribbonReadyArtwork)) return true;
        ArtworkEntry artwork = FindArtwork(page, page.ribbonReadyArtwork);
        if (artwork == null)
        {
            WarnOnce("Ribbon Ready Artwork not found on '" + page.name +
                     "': " + page.ribbonReadyArtwork);
            return false;
        }
        return HasEvent(artwork, page.ribbonReadyEvent);
    }

    private RibbonMode DesiredRibbonMode(BookPage page)
    {
        if (page == null || page.ribbonGroup == null || !IsRibbonReady(page))
            return RibbonMode.None;
        if (page.useOneTimeSpecialRibbon && !IsSpecialRibbonComplete(page))
            return page.specialRibbon != null ? RibbonMode.Special : RibbonMode.None;
        if (FindAvailablePage(currentSpreadIndex, 1) >= 0)
            return page.pageTurnRibbon != null ? RibbonMode.PageTurn : RibbonMode.None;
        return page.closeBookRibbon != null ? RibbonMode.CloseBook : RibbonMode.None;
    }

    private void RefreshRibbon()
    {
        if (isTurningPage || viewingHistory || isEnding) return;
        BookPage page = CurrentBookPage();
        RibbonMode desired = DesiredRibbonMode(page);
        int desiredIndex = desired == RibbonMode.None ? -1 : currentSpreadIndex;
        if (ribbonRoutine != null && requestedRibbonIndex == desiredIndex &&
            requestedRibbonMode == desired) return;
        if (ribbonRoutine == null && shownRibbonIndex == desiredIndex &&
            shownRibbonMode == desired) return;
        StopRibbonTransition();
        requestedRibbonIndex = desiredIndex;
        requestedRibbonMode = desired;
        ribbonRoutine = StartCoroutine(TransitionRibbon(desiredIndex, desired));
    }

    private void SetRibbonChild(BookPage page, RibbonMode mode)
    {
        if (page == null) return;
        // Clear ALL children registered for this parent, even if several
        // spreads reuse the same left/right ribbon group.
        foreach (BookPage other in pages)
        {
            if (other == null || other.ribbonGroup != page.ribbonGroup) continue;
            if (other.specialRibbon != null) other.specialRibbon.SetActive(false);
            if (other.pageTurnRibbon != null) other.pageTurnRibbon.SetActive(false);
            if (other.closeBookRibbon != null) other.closeBookRibbon.SetActive(false);
        }
        GameObject selected = mode == RibbonMode.Special ? page.specialRibbon :
            mode == RibbonMode.PageTurn ? page.pageTurnRibbon :
            mode == RibbonMode.CloseBook ? page.closeBookRibbon : null;
        if (selected != null) selected.SetActive(true);
    }

    private IEnumerator TransitionRibbon(int desiredIndex, RibbonMode desired)
    {
        ribbonTransitioning = true;
        if (shownRibbonIndex >= 0 && shownRibbonIndex < pages.Count)
        {
            BookPage oldPage = pages[shownRibbonIndex];
            CanvasGroup oldCanvas = RibbonCanvas(oldPage);
            if (oldCanvas != null && oldPage.ribbonGroup.activeSelf)
                yield return FadeRibbonCanvas(oldCanvas, 0f, oldPage.ribbonFadeDuration);
            if (oldPage.ribbonGroup != null) oldPage.ribbonGroup.SetActive(false);
        }
        shownRibbonIndex = -1;
        shownRibbonMode = RibbonMode.None;

        if (desiredIndex >= 0 && desiredIndex < pages.Count)
        {
            BookPage newPage = pages[desiredIndex];
            if (newPage != null && newPage.ribbonGroup != null)
            {
                CanvasGroup canvas = RibbonCanvas(newPage);
                SetRibbonChild(newPage, desired);
                newPage.ribbonGroup.SetActive(true);
                canvas.alpha = 0f;
                shownRibbonIndex = desiredIndex;
                shownRibbonMode = desired;
                yield return FadeRibbonCanvas(canvas, 1f, newPage.ribbonFadeDuration);
            }
        }
        ribbonTransitioning = false;
        ribbonRoutine = null;
    }

    private IEnumerator FadeRibbonCanvas(CanvasGroup canvas, float target, float duration)
    {
        if (canvas == null) yield break;
        float start = canvas.alpha;
        float elapsed = 0f;
        float length = Mathf.Max(0f, duration);
        do
        {
            elapsed = Mathf.Min(length, elapsed + Time.unscaledDeltaTime);
            float t = length <= 0f ? 1f : elapsed / length;
            canvas.alpha = Mathf.Lerp(start, target, t);
            if (t >= 1f) break;
            yield return null;
        } while (true);
    }

    private void StopRibbonTransition()
    {
        if (ribbonRoutine != null) StopCoroutine(ribbonRoutine);
        ribbonRoutine = null;
        ribbonTransitioning = false;
    }

    private void StartRibbonFadeOut(float duration)
    {
        StopRibbonTransition();
        if (shownRibbonIndex < 0 || shownRibbonIndex >= pages.Count) return;
        BookPage page = pages[shownRibbonIndex];
        CanvasGroup canvas = RibbonCanvas(page);
        if (canvas != null && page.ribbonGroup.activeSelf)
        {
            requestedRibbonIndex = -1;
            requestedRibbonMode = RibbonMode.None;
            ribbonRoutine = StartCoroutine(FadeRibbonCanvas(canvas, 0f, duration));
        }
    }

    private void HideAllRibbonGroups()
    {
        StopRibbonTransition();
        if (pages != null)
        {
            HashSet<GameObject> hidden = new HashSet<GameObject>();
            foreach (BookPage page in pages)
                if (page != null && page.ribbonGroup != null &&
                    hidden.Add(page.ribbonGroup))
                    page.ribbonGroup.SetActive(false);
        }
        shownRibbonIndex = -1;
        shownRibbonMode = RibbonMode.None;
        requestedRibbonIndex = -1;
        requestedRibbonMode = RibbonMode.None;
    }

    private void ActivateSpecialRibbon(BookPage page)
    {
        if (page == null || IsSpecialRibbonComplete(page) ||
            !IsRibbonReady(page)) return;
        if (page.specialRibbonExitsBook)
        {
            TryContinue(true, true, page);
            return;
        }
        FlorilegiumProgress.MarkRibbonActionCompleted(SpecialRibbonId(page));
        page.onSpecialRibbonActivated?.Invoke();
        RefreshRibbon();
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
        int firstReadable = HasOpeningSpread() ? FindAvailablePage(0, 1) : 0;
        if (viewingHistory || isTurningPage || isEnding ||
            currentSpreadIndex != firstReadable || !CanLeaveCurrentSpread())
            return;

        StartCoroutine(SwitchToHistory());
    }

    private IEnumerator SwitchToHistory()
    {
        isTurningPage = true;
        StartRibbonFadeOut(pageTurnFadeDuration);
        StopActiveAnimations();
        PlayPageTurnSFX();
        yield return FadeCurrentArtworkOut();
        HideAllRibbonGroups();
        viewingHistory = true;
        ActivateSpread(-1);
        isTurningPage = false;
    }

    public void ReturnFromHistory()
    {
        if (!viewingHistory || isTurningPage || isEnding)
            return;

        viewingHistory = false;
        currentSpreadIndex = HasOpeningSpread() ? FindAvailablePage(0, 1) : 0;
        if (currentSpreadIndex < 0) currentSpreadIndex = 0;
        ActivateSpread(currentSpreadIndex);
        StartSpreadArtwork(currentSpreadIndex);
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
        graphics.RemoveAll(graphic => sessionVisibleTargets.Contains(graphic));
        List<CanvasGroup> pageGroups = GetPageCanvasGroups(pages[currentSpreadIndex]);
        pageGroups.RemoveAll(group => sessionVisibleTargets.Contains(group));
        List<float> pageGroupStarts = new List<float>();
        foreach (CanvasGroup group in pageGroups)
            pageGroupStarts.Add(group.alpha);

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
            yield return null;
        }
        foreach (CanvasGroup group in pageGroups)
            if (group != null) group.alpha = 0f;
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

    private void StartSpreadArtwork(int index, int previousIndex = -1)
    {
        StopActiveAnimations();
        if (index < 0 || index >= pages.Count) return;
        BookPage page = pages[index];
        if (page == null) return;
        BookPage previous = previousIndex >= 0 && previousIndex < pages.Count
            ? pages[previousIndex] : null;

        // Book-session artwork and artwork shared by BOTH neighboring spreads
        // are not reset, faded, or animated again during the transition.
        HashSet<ArtworkEntry> retained = new HashSet<ArtworkEntry>();
        if (page.artwork != null)
            foreach (ArtworkEntry entry in page.artwork)
            {
                if (entry == null) continue;
                bool keep = ShouldRetainIncoming(entry, previous);
                if (keep)
                {
                    retained.Add(entry);
                    startedArtwork.Add(entry);
                    completedArtwork.Add(entry);
                    continue;
                }
                CacheOriginalTransform(entry);
                RestoreOriginalTransform(entry);
                if (!ApplyStoredAppearance(entry))
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
            {
                if (entry == null || retained.Contains(entry)) continue;
                if (entry.keepFinalStateOnRevisit && completedAppearances.ContainsKey(entry))
                {
                    startedArtwork.Add(entry);
                    completedArtwork.Add(entry);
                    continue;
                }
                activeAnimations.Add(StartCoroutine(RunArtwork(page, entry, version)));
            }
        if (playKeys)
            activeAnimations.Add(StartCoroutine(RunInteractions(page, index, version)));
    }

    private static UnityEngine.Object ArtworkTarget(ArtworkEntry entry)
    {
        if (entry == null) return null;
        return entry.graphic != null ? (UnityEngine.Object)entry.graphic : entry.canvasGroup;
    }

    private bool HasOpeningSpread()
    {
        return pages != null && pages.Exists(page => page != null && page.isOpeningSpread);
    }

    private bool IsPageAvailable(int index)
    {
        if (pages == null || index < 0 || index >= pages.Count || pages[index] == null)
            return false;
        BookPage page = pages[index];
        // A blank ID is treated as a legacy, always-visible page. This ensures
        // newly added fields never hide older spreads and Inspector assignments.
        if (page.isOpeningSpread || page.availableFromStart ||
            string.IsNullOrWhiteSpace(page.pageId)) return true;
        return FlorilegiumProgress.IsUnlocked(page.pageId);
    }

    private int FindAvailablePage(int startExclusive, int direction)
    {
        int i = startExclusive + direction;
        while (pages != null && i >= 0 && i < pages.Count)
        {
            if (IsPageAvailable(i) &&
                !(direction < 0 && pages[i].isOpeningSpread))
                return i;
            i += direction;
        }
        return -1;
    }

    private bool SharedByBoth(BookPage outgoing, BookPage incoming, UnityEngine.Object target)
    {
        if (outgoing == null || incoming == null || target == null ||
            outgoing.artwork == null || incoming.artwork == null) return false;
        bool a = outgoing.artwork.Exists(e => e != null &&
                 e.keepVisibleWhenShared && ArtworkTarget(e) == target);
        bool b = incoming.artwork.Exists(e => e != null &&
                 e.keepVisibleWhenShared && ArtworkTarget(e) == target);
        return a && b;
    }

    private bool RetainTargetOnTransition(BookPage outgoing, BookPage incoming,
        UnityEngine.Object target)
    {
        if (target == null) return false;
        return sessionVisibleTargets.Contains(target) ||
               SharedByBoth(outgoing, incoming, target);
    }

    private bool ShouldRetainIncoming(ArtworkEntry entry, BookPage previous)
    {
        UnityEngine.Object target = ArtworkTarget(entry);
        return target != null &&
               (sessionVisibleTargets.Contains(target) ||
                SharedByBoth(previous, pages[currentSpreadIndex], target));
    }

    private void SaveCompletedAppearance(ArtworkEntry entry)
    {
        if (entry == null || !entry.keepFinalStateOnRevisit) return;
        StoredAppearance appearance = new StoredAppearance();
        if (entry.graphic != null)
        {
            appearance.hasGraphic = true;
            appearance.graphicColor = entry.graphic.color;
        }
        if (entry.canvasGroup != null)
        {
            appearance.hasGroup = true;
            appearance.groupAlpha = entry.canvasGroup.alpha;
        }
        UnityEngine.Object target = ArtworkTarget(entry);
        if (target is Component component)
        {
            Transform t = component.transform;
            appearance.localScale = t.localScale;
            appearance.localPosition = t.localPosition;
            if (t is RectTransform rect)
            {
                appearance.isRect = true;
                appearance.anchoredPosition = rect.anchoredPosition;
            }
        }
        completedAppearances[entry] = appearance;
    }

    private bool ApplyStoredAppearance(ArtworkEntry entry)
    {
        if (entry == null || !entry.keepFinalStateOnRevisit ||
            !completedAppearances.TryGetValue(entry, out StoredAppearance state))
            return false;
        if (state.hasGraphic && entry.graphic != null)
            entry.graphic.color = state.graphicColor;
        if (state.hasGroup && entry.canvasGroup != null)
            entry.canvasGroup.alpha = state.groupAlpha;
        UnityEngine.Object target = ArtworkTarget(entry);
        if (target is Component component)
        {
            Transform t = component.transform;
            t.localScale = state.localScale;
            if (state.isRect && t is RectTransform rect)
                rect.anchoredPosition = state.anchoredPosition;
            else
                t.localPosition = state.localPosition;
        }
        return true;
    }

    private bool CanLeaveCurrentSpread(bool ignoreOneTimeRibbon = false)
    {
        if (pages == null || currentSpreadIndex < 0 || currentSpreadIndex >= pages.Count)
            return true;
        BookPage page = pages[currentSpreadIndex];
        if (page == null) return true;
        if (!ignoreOneTimeRibbon && page.ribbonGroup != null &&
            page.useOneTimeSpecialRibbon && page.specialRibbon != null &&
            !IsSpecialRibbonComplete(page)) return false;
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
            if (page.autoAdvanceAfterInteractions)
            {
                if (!string.IsNullOrWhiteSpace(page.autoAdvanceAfterArtwork))
                {
                    ArtworkEntry target = FindArtwork(page, page.autoAdvanceAfterArtwork);
                    if (target == null)
                    {
                        WarnOnce("Automatic advance artwork not found: " +
                            page.autoAdvanceAfterArtwork);
                        yield break;
                    }
                    while (version == artworkRunVersion &&
                           !HasEvent(target, page.autoAdvanceAfterEvent))
                        yield return null;
                }
                if (page.autoAdvanceDelay > 0f)
                    yield return new WaitForSecondsRealtime(page.autoAdvanceDelay);
                while (version == artworkRunVersion && !CanLeaveCurrentSpread())
                    yield return null;
                if (version != artworkRunVersion) yield break;
                int next = FindAvailablePage(index, 1);
                if (next >= 0) ShowSpread(next);
            }
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
        if (entry.keepVisibleForBookSession && ArtworkTarget(entry) != null)
            sessionVisibleTargets.Add(ArtworkTarget(entry));
        if (entry.specialAnimation == SpecialAnimation.None ||
            entry.specialAnimation == SpecialAnimation.Steam)
            SaveCompletedAppearance(entry);
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
            SaveCompletedAppearance(entry);
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
        return pages != null && currentSpreadIndex >= 0 &&
               FindAvailablePage(currentSpreadIndex, 1) < 0;
    }

    public void Continue()
    {
        TryContinue(false, false);
    }

    private bool TryContinue(bool ignoreOneTimeRibbon, bool bypassLastSpread,
        BookPage completedSpecialPage = null)
    {
        if (isEnding || isTurningPage || pages == null || pages.Count == 0)
            return false;
        if (!bypassLastSpread && requireLastSpreadForConfirm && !IsOnLastSpread())
            return false;
        if (!CanLeaveCurrentSpread(ignoreOneTimeRibbon)) return false;

        string destination = GetContinueDestination();
        if (string.IsNullOrWhiteSpace(destination))
        {
            Debug.LogError("[IntroBook] Continue destination is empty.", this);
            return false;
        }
        if (!Application.CanStreamedLevelBeLoaded(destination))
        {
            Debug.LogError("[IntroBook] Scene not in build or not loadable: " +
                           destination, this);
            return false;
        }

        // Record the special action and grant its reward before the level
        // handoff, including when all transition delays are set to zero.
        if (completedSpecialPage != null)
        {
            FlorilegiumProgress.MarkRibbonActionCompleted(
                SpecialRibbonId(completedSpecialPage));
            completedSpecialPage.onSpecialRibbonActivated?.Invoke();
        }
        StopActiveAnimations();
        if (objectToDisableOnContinue != null)
            objectToDisableOnContinue.SetActive(false);
        if (continueSFXSource != null && continueSFXClip != null)
            continueSFXSource.PlayOneShot(continueSFXClip);
        StartCoroutine(BeginEndSequence());
        return true;
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

    private string GetContinueDestination()
    {
        return FlorilegiumTravelState.HasReturnDestination &&
               FlorilegiumProgress.FirstVisitCompleted
            ? FlorilegiumTravelState.ReturnSceneName
            : nextLevelName;
    }

    private void HandoffToLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("[IntroBook] LevelManager.Instance is missing.", this);
            return;
        }

        bool returning = FlorilegiumTravelState.HasReturnDestination &&
                         FlorilegiumProgress.FirstVisitCompleted;
        string destination = returning ? FlorilegiumTravelState.ReturnSceneName : nextLevelName;
        if (!returning)
        {
            // The first visit retains the original configured destination.
            FlorilegiumTravelState.ClearReturn();
            FlorilegiumProgress.MarkFirstVisitCompleted();
        }

        if (useSetNextLevelThenGotoNext)
        {
            LevelManager.Instance.SetNextLevel(destination);
            LevelManager.Instance.GotoNextLevel();
        }
        else
        {
            LevelManager.Instance.GotoLevel(destination,
                levelManagerFadeOut, levelManagerSave);
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
