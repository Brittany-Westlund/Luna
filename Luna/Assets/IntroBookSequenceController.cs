
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroBookSequenceController : MonoBehaviour
{
    [Serializable]
    public class FadeItem
    {
        public GameObject target;
        [Min(0f)] public float delay = 0f;
        [Min(0f)] public float duration = 1f;
        [Range(0f, 1f)] public float startingAlpha = 0f;
        [Range(0f, 1f)] public float targetAlpha = 1f;

        [Header("Initial Sound (optional)")]
        public AudioClip initialSFX;
        [Range(0f, 1f)] public float initialVolume = 1f;
        [Range(0f, 1f)] public float initialSFXTriggerAlpha = 0.9f;

        [Header("Ambient Loop (optional)")]
        public AudioClip ambientSFX;
        [Range(0f, 1f)] public float ambientVolume = 1f;
        [Range(0f, 1f)] public float ambientStartAlpha = 0.9f;
    }

    [Header("Interaction Keys")]
    [SerializeField] private KeyCode bookPromptKey = KeyCode.E;
    [SerializeField] private KeyCode placeTeapotKey = KeyCode.E;
    [SerializeField] private KeyCode tearoseKey = KeyCode.E;

    [Header("Scene Opening")]
    [SerializeField] private GameObject levelBackground;
    [SerializeField] private FadeItem closedBook = new FadeItem { startingAlpha = 1f, targetAlpha = 0f };
    [SerializeField] private FadeItem bookPrompt = new FadeItem { startingAlpha = 1f, targetAlpha = 0f };

    [Header("Open Book")]
    [SerializeField] private FadeItem openBook = new FadeItem();
    [SerializeField] private FadeItem lilystool = new FadeItem();
    [SerializeField] private FadeItem placeTeapotPrompt = new FadeItem();

    [Header("Tea Preparation")]
    [SerializeField] private FadeItem teapot = new FadeItem();
    [SerializeField] private FadeItem candle = new FadeItem();
    [SerializeField] private FadeItem dew = new FadeItem { startingAlpha = 1f, targetAlpha = 0f };
    [SerializeField] private FadeItem tearose = new FadeItem();
    [SerializeField] private FadeItem tearosePrompt = new FadeItem();

    [Header("Tea Ready")]
    [SerializeField] private FadeItem teapotBright = new FadeItem();
    [SerializeField] private FadeItem steamOne = new FadeItem();
    [SerializeField] private FadeItem steamTwo = new FadeItem();

    [Header("Final Artwork - Independent Fades")]
    [SerializeField] private FadeItem bookIllustration = new FadeItem();
    [SerializeField] private FadeItem endMark = new FadeItem();
    [SerializeField] private FadeItem titleText = new FadeItem();
    [SerializeField] private FadeItem moonbow = new FadeItem();
    [SerializeField] private FadeItem leftPageText = new FadeItem();
    [SerializeField] private FadeItem rightPageText = new FadeItem();
    [SerializeField] private FadeItem controlsText = new FadeItem();

    [Header("Prompt Key Sound Effects")]
    [SerializeField] private AudioClip bookPromptSFX;
    [Range(0f, 1f)] [SerializeField] private float bookPromptVolume = 1f;
    [SerializeField] private AudioClip placeTeapotSFX;
    [Range(0f, 1f)] [SerializeField] private float placeTeapotVolume = 1f;
    [SerializeField] private AudioClip tearosePromptSFX;
    [Range(0f, 1f)] [SerializeField] private float tearosePromptVolume = 1f;

    [Header("Spread Controller Handoff")]
    [SerializeField] private IntroBookSpreadSequenceController spreadController;
    [Min(0f)] [SerializeField] private float bookControllerStartDelay = 0f;

    private enum Stage
    {
        BookPrompt,
        Opening,
        TeapotPrompt,
        Preparing,
        TearosePrompt,
        Finishing,
        Finished
    }

    private Stage stage = Stage.BookPrompt;

    private readonly Dictionary<GameObject, CanvasGroup> groups =
        new Dictionary<GameObject, CanvasGroup>();

    private readonly Dictionary<GameObject, List<AudioSource>> attachedSounds =
        new Dictionary<GameObject, List<AudioSource>>();

    private AudioSource buttonSource;

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
    }

    private void Update()
    {
        switch (stage)
        {
            case Stage.BookPrompt:
                if (Input.GetKeyDown(bookPromptKey))
                    PressBookPrompt();
                break;

            case Stage.TeapotPrompt:
                if (Input.GetKeyDown(placeTeapotKey))
                    PressPlaceTeapotPrompt();
                break;

            case Stage.TearosePrompt:
                if (Input.GetKeyDown(tearoseKey))
                    PressTearosePrompt();
                break;
        }
    }

    public void PressBookPrompt()
    {
        if (stage != Stage.BookPrompt)
            return;

        stage = Stage.Opening;
        PlayButton(bookPromptSFX, bookPromptVolume);
        StartCoroutine(BookOpening());
    }

    public void PressPlaceTeapotPrompt()
    {
        if (stage != Stage.TeapotPrompt)
            return;

        stage = Stage.Preparing;
        PlayButton(placeTeapotSFX, placeTeapotVolume);
        StartCoroutine(TeapotPreparation());
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
        // Open book remains visible; closed book and prompt disappear.
        yield return RunParallel(closedBook, bookPrompt, openBook);

        // Each delay begins after the preceding fade completes.
        yield return Fade(lilystool);
        yield return Fade(placeTeapotPrompt);

        stage = Stage.TeapotPrompt;
    }

    private IEnumerator TeapotPreparation()
    {
        // Teapot and candle delays begin at the key press.
        StartCoroutine(Fade(placeTeapotPrompt, 0f));
        StartCoroutine(Fade(candle));

        yield return Fade(teapot);

        // These delays begin once the teapot reaches full alpha.
        StartCoroutine(Fade(dew));
        yield return Fade(tearose);
        yield return Fade(tearosePrompt);

        stage = Stage.TearosePrompt;
    }

    private IEnumerator FinishIntroduction()
    {
        // Final fades run independently from the Tearose key press.
        yield return RunParallelFinal(
            teapotBright,
            steamOne,
            steamTwo,
            bookIllustration,
            endMark,
            titleText,
            moonbow,
            leftPageText,
            rightPageText,
            controlsText
        );

        if (bookControllerStartDelay > 0f)
            yield return new WaitForSecondsRealtime(bookControllerStartDelay);

        stage = Stage.Finished;

        if (spreadController != null)
        {
            spreadController.enabled = true;
        }
        else
        {
            Debug.LogWarning(
                "[IntroBookSequence] Spread controller is not assigned.",
                this
            );
        }
    }

    private IEnumerator RunParallelFinal(params FadeItem[] items)
    {
        List<FadeItem> active = new List<FadeItem>();

        foreach (FadeItem item in items)
        {
            if (item != null && item.target != null)
                active.Add(item);
        }

        int remaining = active.Count;

        if (tearose != null && tearose.target != null)
            remaining++;

        if (tearosePrompt != null && tearosePrompt.target != null)
            remaining++;

        if (tearose != null && tearose.target != null)
        {
            StartCoroutine(
                FadeAndSignal(tearose, 0f, () => remaining--)
            );
        }

        if (tearosePrompt != null && tearosePrompt.target != null)
        {
            StartCoroutine(
                FadeAndSignal(tearosePrompt, 0f, () => remaining--)
            );
        }

        foreach (FadeItem item in active)
        {
            StartCoroutine(
                FadeAndSignal(item, null, () => remaining--)
            );
        }

        while (remaining > 0)
            yield return null;
    }

    private IEnumerator RunParallel(params FadeItem[] items)
    {
        int remaining = 0;

        foreach (FadeItem item in items)
        {
            if (item == null || item.target == null)
                continue;

            remaining++;

            StartCoroutine(
                FadeAndSignal(item, null, () => remaining--)
            );
        }

        while (remaining > 0)
            yield return null;
    }

    private IEnumerator FadeAndSignal(
        FadeItem item,
        float? target,
        Action onDone
    )
    {
        yield return Fade(item, target);
        onDone?.Invoke();
    }

    private IEnumerator Fade(
        FadeItem item,
        float? overrideTarget = null
    )
    {
        if (item == null || item.target == null)
            yield break;

        CanvasGroup group = GetGroup(item.target);

        if (item.delay > 0f)
            yield return new WaitForSecondsRealtime(item.delay);

        float from = group.alpha;
        float to = overrideTarget ?? item.targetAlpha;
        bool fadingOut = to < from;

        float duration = Mathf.Max(0f, item.duration);
        float elapsed = 0f;

        bool initialPlayed = false;
        bool ambientPlayed = false;

        List<AudioSource> sounds = GetSounds(item.target);

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
                if (!initialPlayed &&
                    item.initialSFX != null &&
                    alpha >= item.initialSFXTriggerAlpha)
                {
                    PlayAttached(
                        item.target,
                        item.initialSFX,
                        item.initialVolume,
                        false
                    );

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
                        true
                    );

                    ambientPlayed = true;
                }
            }
            else
            {
                float fraction = from <= 0f
                    ? 0f
                    : Mathf.Clamp01(alpha / from);

                foreach (
                    KeyValuePair<AudioSource, float> pair
                    in initialVolumes
                )
                {
                    if (pair.Key != null)
                        pair.Key.volume = pair.Value * fraction;
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

    private CanvasGroup GetGroup(GameObject target)
    {
        if (!groups.TryGetValue(
                target,
                out CanvasGroup group
            ) || group == null)
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
                out List<AudioSource> list
            ))
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
        bool loop
    )
    {
        if (clip == null)
            return;

        // Sources are created automatically on this controller.
        AudioSource source = gameObject.AddComponent<AudioSource>();

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
                CleanupOneShot(target, source, clip.length)
            );
        }
    }

    private IEnumerator CleanupOneShot(
        GameObject target,
        AudioSource source,
        float duration
    )
    {
        yield return new WaitForSecondsRealtime(duration + 0.1f);

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

    private void PlayButton(AudioClip clip, float volume)
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
        yield return placeTeapotPrompt;
        yield return teapot;
        yield return candle;
        yield return dew;
        yield return tearose;
        yield return tearosePrompt;
        yield return teapotBright;
        yield return steamOne;
        yield return steamTwo;
        yield return bookIllustration;
        yield return endMark;
        yield return titleText;
        yield return moonbow;
        yield return leftPageText;
        yield return rightPageText;
        yield return controlsText;
    }
}
