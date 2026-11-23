
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroBookSequenceController : MonoBehaviour
{
    [Serializable]
    public class FadeItem
    {
        [Tooltip("Object controlled by this fade.")]
        public GameObject target;

        [Tooltip("Time between this item's trigger and the start of its fade.")]
        [Min(0f)] public float delay = 0f;

        [Tooltip("Time this item takes to fade to Target Alpha.")]
        [Min(0f)] public float duration = 1f;

        [Tooltip("Alpha assigned when the intro initializes.")]
        [Range(0f, 1f)] public float startingAlpha = 0f;

        [Tooltip("Alpha reached when this item's fade-in finishes.")]
        [Range(0f, 1f)] public float targetAlpha = 1f;

        [Tooltip("Sound played once when this item's alpha reaches Initial SFX Trigger Alpha.")]
        public AudioClip initialSFX;

        [Tooltip("Volume of Initial SFX.")]
        [Range(0f, 1f)] public float initialVolume = 1f;

        [Tooltip("Initial SFX plays when this item's alpha reaches this value.")]
        [Range(0f, 1f)] public float initialSFXTriggerAlpha = 0.9f;

        [Tooltip("Looping sound started when this item's alpha reaches Ambient Start Alpha.")]
        public AudioClip ambientSFX;

        [Tooltip("Volume of Ambient SFX.")]
        [Range(0f, 1f)] public float ambientVolume = 1f;

        [Tooltip("Ambient SFX starts when this item's alpha reaches this value.")]
        [Range(0f, 1f)] public float ambientStartAlpha = 0.9f;
    }

    [Serializable]
    public class PulseSettings
    {
        [Tooltip("Image animated by these settings. Its CanvasGroup controls overall visibility.")]
        public Graphic graphic;

        [Tooltip("Lowest Image alpha during the repeating animation.")]
        [Range(0f, 1f)] public float minimumAlpha = 0.65f;

        [Tooltip("Highest Image alpha during the repeating animation.")]
        [Range(0f, 1f)] public float maximumAlpha = 1f;

        [Tooltip("Seconds for one complete animation cycle.")]
        [Min(0.01f)] public float cycleDuration = 2f;

        [Tooltip("Offsets this Image's cycle relative to the other steam Image.")]
        public float phaseOffset = 0f;
    }

    [Header("Interaction Keys")]

    [Tooltip("First E press starts Book Opening.")]
    [SerializeField] private KeyCode bookPromptKey = KeyCode.E;

    [Tooltip("Second E press starts Finish Introduction after Tearose Prompt appears.")]
    [SerializeField] private KeyCode tearoseKey = KeyCode.E;

    [Header("Scene Opening")]

    [Tooltip("Activated in Awake, before the first E press.")]
    [SerializeField] private GameObject levelBackground;

    [Tooltip("Fades out on the first E press.")]
    [SerializeField] private FadeItem closedBook =
        new FadeItem { startingAlpha = 1f, targetAlpha = 0f };

    [Tooltip("Fades out on the first E press.")]
    [SerializeField] private FadeItem bookPrompt =
        new FadeItem { startingAlpha = 1f, targetAlpha = 0f };

    [Header("Open Book")]

    [Tooltip("Starts fading in on the first E press. Does not control Lilystool or Dew timing.")]
    [SerializeField] private FadeItem openBook = new FadeItem();

    [Tooltip("Starts on the first E press. Its Delay and Duration also control Dew's entrance.")]
    [SerializeField] private FadeItem lilystool = new FadeItem();

    [Header("Teapot")]

    [Tooltip("Starts on the first E press. Tearose waits for Teapot and the Lilystool/Dew entrance.")]
    [SerializeField] private FadeItem teapot = new FadeItem();

    [Tooltip("Starts with Teapot on the first E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem teapotShadow =
        new FadeItem { targetAlpha = 45f / 255f };

    [Tooltip("Fades in on the second E press. Bright Teapot Fade Out starts after this fade-in finishes.")]
    [SerializeField] private FadeItem teapotBright = new FadeItem();

    [Header("Candle")]

    [Tooltip("Fades in on the first E press. Candle Flame animates continuously.")]
    [SerializeField] private FadeItem candle = new FadeItem();

    [Header("Candle Flame - Continuous Animation")]

    [Tooltip("Image animated continuously inside Candle. Candle's CanvasGroup controls visibility.")]
    [SerializeField] private Graphic candleFlame;

    [Tooltip("Enables continuous Candle Flame animation in Update.")]
    [SerializeField] private bool animateCandleFlame = true;

    [Tooltip("Lowest Candle Flame alpha, multiplied by its original Image alpha.")]
    [Range(0f, 1f)]
    [SerializeField] private float flameMinimumAlpha = 0.8f;

    [Tooltip("Highest Candle Flame alpha, multiplied by its original Image alpha.")]
    [Range(0f, 1f)]
    [SerializeField] private float flameMaximumAlpha = 1f;

    [Tooltip("Seconds for one Candle Flame animation cycle.")]
    [Min(0.01f)]
    [SerializeField] private float flameCycleDuration = 1.5f;

    [Tooltip("Smallest Candle Flame scale relative to its original scale.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float flameMinimumScale = 0.94f;

    [Tooltip("Maximum distance Candle Flame moves downward during each cycle.")]
    [Min(0f)]
    [SerializeField] private float flameDropDistance = 3f;

    [Header("Dew - Paired Entrance With Lilystool")]

    [Tooltip("Fades in with Lilystool. Lilystool's Delay and Duration control both entrances. Dew uses its own Target Alpha.")]
    [SerializeField] private FadeItem dew = new FadeItem();

    [Header("Dew - Fade Out, Shrink and Drop")]

    [Tooltip("Starts after Teapot and the Lilystool/Dew entrance finish. Steam is triggered when this delay ends.")]
    [Min(0f)]
    [SerializeField] private float dewFadeOutDelay = 0f;

    [Tooltip("Controls Dew's fade-out, shrink, and drop duration. Does not control Steam.")]
    [Min(0f)]
    [SerializeField] private float dewFadeOutDuration = 1f;

    [Tooltip("Dew's final scale relative to its original scale.")]
    [Range(0f, 1f)]
    [SerializeField] private float dewEndScale = 0.5f;

    [Tooltip("Distance Dew moves downward during its exit.")]
    [Min(0f)]
    [SerializeField] private float dewDropDistance = 30f;

    [Header("Tearose")]

    [Tooltip("Starts after Teapot and the Lilystool/Dew entrance finish. Does not wait for Dew's exit or Steam.")]
    [SerializeField] private FadeItem tearose = new FadeItem();

    [Tooltip("Starts after Tearose finishes fading in. Enables the second E press.")]
    [SerializeField] private FadeItem tearosePrompt = new FadeItem();

    [Header("Steam - Starts With Dew Exit")]

    [Tooltip("Triggered when Dew begins fading out. Uses its own Delay and Duration. Animation continues after fade-in.")]
    [SerializeField] private FadeItem steamOne = new FadeItem();

    [Tooltip("Triggered when Dew begins fading out. Uses its own Delay and Duration. Animation continues after fade-in.")]
    [SerializeField] private FadeItem steamTwo = new FadeItem();

    [Header("Steam - Continuous Animation")]

    [Tooltip("Enables continuous Steam One and Steam Two animation in Update.")]
    [SerializeField] private bool animateSteam = true;

    [Tooltip("Controls Steam One's repeating Image alpha. Steam One Fade controls initial visibility.")]
    [SerializeField] private PulseSettings steamOnePulse =
        new PulseSettings();

    [Tooltip("Controls Steam Two's repeating Image alpha. Steam Two Fade controls initial visibility.")]
    [SerializeField] private PulseSettings steamTwoPulse =
        new PulseSettings
        {
            minimumAlpha = 0.55f,
            maximumAlpha = 1f,
            cycleDuration = 2.7f,
            phaseOffset = 0.5f
        };

    [Header("Flowers Inside Teapot")]

    [Tooltip("Fades in on the second E press. Teapot Flowers Fade Out waits only for this fade-in.")]
    [SerializeField] private FadeItem teapotFlowers = new FadeItem();

    [Header("Tea Ready Fade Outs")]

    [Tooltip("Starts after Teapot Flowers finishes fading in. Independent of Bright Teapot. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem teapotFlowersFadeOut =
        new FadeItem { targetAlpha = 0f };

    [Tooltip("Starts after Bright Teapot finishes fading in. Independent of Teapot Flowers. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem teapotBrightFadeOut =
        new FadeItem { targetAlpha = 0f };

    [Header("Final Artwork - Second E Press")]

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem bookIllustration = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem endMark = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem titleText = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem moonbow = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem leftPageText = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem rightPageText = new FadeItem();

    [Tooltip("Starts on the second E press. Uses its own Delay and Duration.")]
    [SerializeField] private FadeItem controlsText = new FadeItem();

    [Header("Prompt Sounds")]

    [Tooltip("Plays when the first E press starts Book Opening.")]
    [SerializeField] private AudioClip bookPromptSFX;

    [Tooltip("Volume of Book Prompt SFX.")]
    [Range(0f, 1f)]
    [SerializeField] private float bookPromptVolume = 1f;

    [Tooltip("Plays when the second E press starts Finish Introduction.")]
    [SerializeField] private AudioClip tearosePromptSFX;

    [Tooltip("Volume of Tearose Prompt SFX.")]
    [Range(0f, 1f)]
    [SerializeField] private float tearosePromptVolume = 1f;

    [Header("Spread Controller Handoff")]

    [Tooltip("Enabled after the second-E sequence and Book Controller Start Delay finish.")]
    [SerializeField] private IntroBookSpreadSequenceController spreadController;

    [Tooltip("Additional wait after the second-E sequence before enabling Spread Controller.")]
    [Min(0f)]
    [SerializeField] private float bookControllerStartDelay = 0f;

    [Header("First Spread - Return Arrangement")]

    [Tooltip("Controls arrangement fades when leaving or returning to spread 0. Does not restart Steam or Candle animation.")]
    [Min(0f)]
    [SerializeField] private float arrangementFadeDuration = 1f;

    private enum Stage
    {
        BookPrompt,
        Opening,
        TearosePrompt,
        Finishing,
        Finished
    }

    private Stage stage = Stage.BookPrompt;

    private readonly Dictionary<GameObject, CanvasGroup> groups =
        new Dictionary<GameObject, CanvasGroup>();

    private readonly Dictionary<GameObject, List<AudioSource>> attachedSounds =
        new Dictionary<GameObject, List<AudioSource>>();

    private readonly Dictionary<GameObject, int> activeFadeCounts =
        new Dictionary<GameObject, int>();

    private AudioSource buttonSource;

    private bool pairedEntranceComplete;
    private bool hasLeftFirstSpread;
    private int arrangementVersion;

    private Coroutine dewExitRoutine;

    private RectTransform dewRect;
    private Vector3 dewOriginalScale;
    private Vector3 dewOriginalLocalPosition;
    private Vector2 dewOriginalAnchoredPosition;
    private bool dewCached;

    private RectTransform flameRect;
    private Vector3 flameOriginalScale;
    private Vector3 flameOriginalLocalPosition;
    private Vector2 flameOriginalAnchoredPosition;
    private float flameOriginalAlpha;
    private bool flameCached;

    private void Awake()
    {
        if (spreadController != null)
            spreadController.enabled = false;

        buttonSource = gameObject.AddComponent<AudioSource>();
        buttonSource.playOnAwake = false;
        buttonSource.spatialBlend = 0f;

        if (levelBackground != null)
            levelBackground.SetActive(true);

        foreach (FadeItem item in AllItems())
        {
            if (item == null || item.target == null)
                continue;

            CanvasGroup group = GetGroup(item.target);
            group.alpha = item.startingAlpha;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        SetAlpha(teapotBright, 0f);
        SetAlpha(teapotFlowers, 0f);
        SetAlpha(teapotShadow, 0f);
        SetAlpha(dew, 0f);
        SetAlpha(steamOne, 0f);
        SetAlpha(steamTwo, 0f);

        CacheDewTransform();
        CacheFlameTransform();

        if (steamOnePulse.graphic == null && steamOne.target != null)
            steamOnePulse.graphic =
                steamOne.target.GetComponent<Graphic>();

        if (steamTwoPulse.graphic == null && steamTwo.target != null)
            steamTwoPulse.graphic =
                steamTwo.target.GetComponent<Graphic>();
    }

    private void Update()
    {
        if (stage == Stage.BookPrompt &&
            Input.GetKeyDown(bookPromptKey))
        {
            PressBookPrompt();
        }
        else if (stage == Stage.TearosePrompt &&
                 Input.GetKeyDown(tearoseKey))
        {
            PressTearosePrompt();
        }

        UpdateSteam();
        UpdateCandleFlame();
    }

    public void PressBookPrompt()
    {
        if (stage != Stage.BookPrompt)
            return;

        stage = Stage.Opening;
        PlayButton(bookPromptSFX, bookPromptVolume);
        StartCoroutine(BookOpening());
    }

    public void PressTearosePrompt()
    {
        if (stage != Stage.TearosePrompt)
            return;

        stage = Stage.Finishing;
        PlayButton(tearosePromptSFX, tearosePromptVolume);
        StartCoroutine(FinishIntroduction());
    }

    private IEnumerator BookOpening()
    {
        StartFade(closedBook);
        StartFade(bookPrompt);
        StartFade(openBook);
        StartFade(candle);
        StartFade(teapot);
        StartFade(teapotShadow);

        pairedEntranceComplete = false;
        StartCoroutine(FadeLilystoolAndDewTogether());

        yield return WaitForFade(teapot);

        while (!pairedEntranceComplete)
            yield return null;

        dewExitRoutine = StartCoroutine(AnimateDewOut());

        yield return Fade(tearose);
        yield return Fade(tearosePrompt);

        stage = Stage.TearosePrompt;
    }

    private IEnumerator FadeLilystoolAndDewTogether()
    {
        GameObject lilyTarget = lilystool.target;
        GameObject dewTarget = dew.target;

        RegisterFade(lilyTarget);
        RegisterFade(dewTarget);

        if (lilystool.delay > 0f)
            yield return new WaitForSecondsRealtime(
                lilystool.delay);

        CanvasGroup lilyGroup = lilyTarget != null
            ? GetGroup(lilyTarget)
            : null;

        CanvasGroup dewGroup = dewTarget != null
            ? GetGroup(dewTarget)
            : null;

        float lilyFrom = lilyGroup != null
            ? lilyGroup.alpha
            : 0f;

        float dewFrom = dewGroup != null
            ? dewGroup.alpha
            : 0f;

        float duration = Mathf.Max(0f, lilystool.duration);
        float elapsed = 0f;

        bool lilyInitial = false;
        bool lilyAmbient = false;
        bool dewInitial = false;
        bool dewAmbient = false;

        while (true)
        {
            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            if (lilyGroup != null)
            {
                lilyGroup.alpha = Mathf.Lerp(
                    lilyFrom,
                    lilystool.targetAlpha,
                    t);

                TriggerEntranceSounds(
                    lilystool,
                    lilyGroup.alpha,
                    ref lilyInitial,
                    ref lilyAmbient);
            }

            if (dewGroup != null)
            {
                dewGroup.alpha = Mathf.Lerp(
                    dewFrom,
                    dew.targetAlpha,
                    t);

                TriggerEntranceSounds(
                    dew,
                    dewGroup.alpha,
                    ref dewInitial,
                    ref dewAmbient);
            }

            if (t >= 1f)
                break;

            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        UnregisterFade(lilyTarget);
        UnregisterFade(dewTarget);

        pairedEntranceComplete = true;
    }

    private IEnumerator AnimateDewOut()
    {
        if (dewFadeOutDelay > 0f)
            yield return new WaitForSecondsRealtime(
                dewFadeOutDelay);

        StartFade(steamOne);
        StartFade(steamTwo);

        if (dew.target == null)
        {
            dewExitRoutine = null;
            yield break;
        }

        CacheDewTransform();

        CanvasGroup group = GetGroup(dew.target);
        Transform target = dew.target.transform;

        float fromAlpha = group.alpha;
        Vector3 fromScale = target.localScale;
        Vector3 toScale = dewOriginalScale * dewEndScale;

        Vector3 fromPosition = target.localPosition;
        Vector3 toPosition =
            dewOriginalLocalPosition +
            Vector3.down * dewDropDistance;

        Vector2 fromAnchored = Vector2.zero;
        Vector2 toAnchored = Vector2.zero;

        if (dewRect != null)
        {
            fromAnchored = dewRect.anchoredPosition;
            toAnchored =
                dewOriginalAnchoredPosition +
                Vector2.down * dewDropDistance;
        }

        float duration = Mathf.Max(0f, dewFadeOutDuration);
        float elapsed = 0f;

        while (true)
        {
            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            group.alpha = Mathf.Lerp(fromAlpha, 0f, t);
            target.localScale =
                Vector3.Lerp(fromScale, toScale, t);

            if (dewRect != null)
            {
                dewRect.anchoredPosition =
                    Vector2.Lerp(fromAnchored, toAnchored, t);
            }
            else
            {
                target.localPosition =
                    Vector3.Lerp(fromPosition, toPosition, t);
            }

            if (t >= 1f)
                break;

            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        group.alpha = 0f;
        StopAttached(dew.target);
        dewExitRoutine = null;
    }

    private IEnumerator FinishIntroduction()
    {
        StartFade(tearose, 0f);
        StartFade(tearosePrompt, 0f);

        StartFade(teapotFlowers);
        StartFade(teapotBright);

        StartFade(bookIllustration);
        StartFade(endMark);
        StartFade(titleText);
        StartFade(moonbow);
        StartFade(leftPageText);
        StartFade(rightPageText);
        StartFade(controlsText);

        // Each exit waits only for its own entrance.
        // Neither exit depends on the other object's timing.
        Coroutine flowersExit = StartCoroutine(
            FadeOutAfterEntrance(
                teapotFlowers,
                teapotFlowersFadeOut));

        Coroutine brightExit = StartCoroutine(
            FadeOutAfterEntrance(
                teapotBright,
                teapotBrightFadeOut));

        // Unlock page turning once Controls Text finishes fading in.
        // Other artwork and exit animations continue independently.
        yield return WaitForFade(controlsText);

        if (bookControllerStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                bookControllerStartDelay);
        }

        stage = Stage.Finished;

        if (spreadController != null)
        {
            spreadController.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "[IntroBookSequence] Spread controller is not assigned.",
                this);
        }
    }

    private IEnumerator FadeOutAfterEntrance(
        FadeItem entrance,
        FadeItem exit)
    {
        // Wait only for this object's fade-in.
        yield return WaitForFade(entrance);

        if (exit == null)
            yield break;

        // Use the entrance target if no separate exit target is assigned.
        if (exit.target == null && entrance != null)
            exit.target = entrance.target;

        if (exit.target == null)
            yield break;

        // Run this object's own exit delay and duration.
        yield return FadeTracked(exit, 0f);
    }

    private void StartConfiguredFadeOut(
        FadeItem fadeOut,
        FadeItem original)
    {
        if (fadeOut == null)
            return;

        if (fadeOut.target == null && original != null)
            fadeOut.target = original.target;

        StartFade(fadeOut, 0f);
    }

    // After the handoff, the spread controller owns all page navigation and revisits.
    // Continuous steam and candle animation remain active in Update.
    public void OnIntroductionPageLeft()
    {
        if (stage != Stage.Finished) return;
        StopAllCoroutines();
        activeFadeCounts.Clear();
    }

    public void RestoreFirstSpreadDewPosition()
    {
        if (stage == Stage.Finished)
            RestoreDewTransform();
    }

    private void UpdateSteam()
    {
        if (!animateSteam)
            return;

        UpdatePulse(steamOnePulse);
        UpdatePulse(steamTwoPulse);
    }

    private void UpdatePulse(PulseSettings settings)
    {
        if (settings == null || settings.graphic == null)
            return;

        float cycle = Mathf.Max(
            0.01f,
            settings.cycleDuration);

        float wave = 0.5f + 0.5f * Mathf.Sin(
            (Time.unscaledTime / cycle +
             settings.phaseOffset) *
            Mathf.PI * 2f);

        float alpha = Mathf.Lerp(
            settings.minimumAlpha,
            settings.maximumAlpha,
            wave);

        Color color = settings.graphic.color;
        color.a = alpha;
        settings.graphic.color = color;
    }

    private void UpdateCandleFlame()
    {
        if (!animateCandleFlame || candleFlame == null)
            return;

        CacheFlameTransform();

        float cycle = Mathf.Max(
            0.01f,
            flameCycleDuration);

        float wave = 0.5f + 0.5f * Mathf.Sin(
            Time.unscaledTime / cycle *
            Mathf.PI * 2f);

        Color color = candleFlame.color;

        color.a = flameOriginalAlpha *
            Mathf.Lerp(
                flameMinimumAlpha,
                flameMaximumAlpha,
                wave);

        candleFlame.color = color;

        float scale = Mathf.Lerp(
            flameMinimumScale,
            1f,
            wave);

        candleFlame.transform.localScale =
            flameOriginalScale * scale;

        float drop = (1f - wave) * flameDropDistance;

        if (flameRect != null)
        {
            flameRect.anchoredPosition =
                flameOriginalAnchoredPosition +
                Vector2.down * drop;
        }
        else
        {
            candleFlame.transform.localPosition =
                flameOriginalLocalPosition +
                Vector3.down * drop;
        }
    }

    private void CacheDewTransform()
    {
        if (dewCached || dew == null || dew.target == null)
            return;

        Transform target = dew.target.transform;

        dewOriginalScale = target.localScale;
        dewOriginalLocalPosition = target.localPosition;

        dewRect = target as RectTransform;

        if (dewRect != null)
            dewOriginalAnchoredPosition =
                dewRect.anchoredPosition;

        dewCached = true;
    }

    private void RestoreDewTransform()
    {
        if (!dewCached || dew == null || dew.target == null)
            return;

        Transform target = dew.target.transform;

        target.localScale = dewOriginalScale;

        if (dewRect != null)
            dewRect.anchoredPosition =
                dewOriginalAnchoredPosition;
        else
            target.localPosition =
                dewOriginalLocalPosition;
    }

    private void CacheFlameTransform()
    {
        if (flameCached || candleFlame == null)
            return;

        Transform target = candleFlame.transform;

        flameOriginalScale = target.localScale;
        flameOriginalLocalPosition = target.localPosition;
        flameOriginalAlpha = candleFlame.color.a;

        flameRect = target as RectTransform;

        if (flameRect != null)
            flameOriginalAnchoredPosition =
                flameRect.anchoredPosition;

        flameCached = true;
    }

    private void StartFade(
        FadeItem item,
        float? overrideTarget = null)
    {
        if (item == null || item.target == null)
            return;

        StartCoroutine(FadeTracked(item, overrideTarget));
    }

    private IEnumerator FadeTracked(
        FadeItem item,
        float? overrideTarget)
    {
        GameObject target = item.target;

        RegisterFade(target);

        yield return Fade(item, overrideTarget);

        UnregisterFade(target);
    }

    private void RegisterFade(GameObject target)
    {
        if (target == null)
            return;

        if (!activeFadeCounts.ContainsKey(target))
            activeFadeCounts[target] = 0;

        activeFadeCounts[target]++;
    }

    private void UnregisterFade(GameObject target)
    {
        if (target == null ||
            !activeFadeCounts.ContainsKey(target))
            return;

        activeFadeCounts[target]--;

        if (activeFadeCounts[target] <= 0)
            activeFadeCounts.Remove(target);
    }

    private IEnumerator WaitForFade(FadeItem item)
    {
        if (item == null || item.target == null)
            yield break;

        while (activeFadeCounts.ContainsKey(item.target))
            yield return null;
    }

    private IEnumerator WaitForAllFades()
    {
        while (activeFadeCounts.Count > 0)
            yield return null;
    }

    private IEnumerator Fade(
        FadeItem item,
        float? overrideTarget = null)
    {
        if (item == null || item.target == null)
            yield break;

        if (item.delay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                item.delay);
        }

        CanvasGroup group = GetGroup(item.target);

        float from = group.alpha;
        float to = overrideTarget ?? item.targetAlpha;
        float duration = Mathf.Max(0f, item.duration);
        bool fadingOut = to < from;

        float elapsed = 0f;
        bool initialPlayed = false;
        bool ambientPlayed = false;

        List<AudioSource> sounds =
            GetSounds(item.target);

        Dictionary<AudioSource, float> initialVolumes =
            new Dictionary<AudioSource, float>();

        foreach (AudioSource source in sounds)
        {
            if (source != null)
                initialVolumes[source] = source.volume;
        }

        while (true)
        {
            float t = duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            float alpha = Mathf.Lerp(from, to, t);
            group.alpha = alpha;

            if (!fadingOut)
            {
                TriggerEntranceSounds(
                    item,
                    alpha,
                    ref initialPlayed,
                    ref ambientPlayed);
            }
            else
            {
                float fraction = from <= 0f
                    ? 0f
                    : Mathf.Clamp01(alpha / from);

                foreach (var pair in initialVolumes)
                {
                    if (pair.Key != null)
                        pair.Key.volume =
                            pair.Value * fraction;
                }
            }

            if (t >= 1f)
                break;

            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        if (fadingOut && to <= 0f)
            StopAttached(item.target);
    }

    private void TriggerEntranceSounds(
        FadeItem item,
        float alpha,
        ref bool initialPlayed,
        ref bool ambientPlayed)
    {
        if (item.target == null)
            return;

        if (!initialPlayed &&
            item.initialSFX != null &&
            alpha >= item.initialSFXTriggerAlpha)
        {
            PlayAttached(
                item.target,
                item.initialSFX,
                item.initialVolume,
                false);

            initialPlayed = true;
        }

        if (!ambientPlayed &&
            item.ambientSFX != null &&
            alpha >= item.ambientStartAlpha)
        {
            PlayAttached(
                item.target,
                item.ambientSFX,
                item.ambientVolume,
                true);

            ambientPlayed = true;
        }
    }

    private void SetAlpha(FadeItem item, float alpha)
    {
        if (item == null || item.target == null)
            return;

        GetGroup(item.target).alpha = alpha;
    }

    private CanvasGroup GetGroup(GameObject target)
    {
        if (!groups.TryGetValue(
                target,
                out CanvasGroup group) ||
            group == null)
        {
            group = target.GetComponent<CanvasGroup>();

            if (group == null)
                group = target.AddComponent<CanvasGroup>();

            groups[target] = group;
        }

        return group;
    }

    private List<AudioSource> GetSounds(GameObject target)
    {
        if (!attachedSounds.TryGetValue(
                target,
                out List<AudioSource> list))
        {
            list = new List<AudioSource>();
            attachedSounds[target] = list;
        }

        return list;
    }

    private void PlayAttached(
        GameObject target,
        AudioClip clip,
        float volume,
        bool loop)
    {
        if (clip == null)
            return;

        AudioSource source =
            gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.Play();

        GetSounds(target).Add(source);

        if (!loop)
        {
            StartCoroutine(
                CleanupOneShot(
                    target,
                    source,
                    clip.length));
        }
    }

    private IEnumerator CleanupOneShot(
        GameObject target,
        AudioSource source,
        float duration)
    {
        yield return new WaitForSecondsRealtime(
            duration + 0.1f);

        GetSounds(target).Remove(source);

        if (source != null)
            Destroy(source);
    }

    private void StopAttached(GameObject target)
    {
        List<AudioSource> list = GetSounds(target);

        foreach (AudioSource source in list)
        {
            if (source == null)
                continue;

            source.Stop();
            Destroy(source);
        }

        list.Clear();
    }

    private void PlayButton(
        AudioClip clip,
        float volume)
    {
        if (clip != null)
            buttonSource.PlayOneShot(clip, volume);
    }

    private IEnumerable<FadeItem> AllItems()
    {
        yield return closedBook;
        yield return bookPrompt;
        yield return openBook;
        yield return lilystool;
        yield return teapot;
        yield return teapotShadow;
        yield return teapotBright;
        yield return candle;
        yield return dew;
        yield return tearose;
        yield return tearosePrompt;
        yield return steamOne;
        yield return steamTwo;
        yield return teapotFlowers;
        yield return bookIllustration;
        yield return endMark;
        yield return titleText;
        yield return moonbow;
        yield return leftPageText;
        yield return rightPageText;
        yield return controlsText;
    }
}
