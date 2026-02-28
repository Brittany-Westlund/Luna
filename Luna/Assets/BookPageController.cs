using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class BookPageController : MonoBehaviour
{
    public static BookPageController Instance { get; private set; }

    [Header("Persistence")]
    public bool dontDestroyOnLoad = true;
    public string saveKeyPrefix = "BOOK_";

    [Header("Renderer Lookup (if not assigned)")]
    public string baseRendererChildName = "Book blank";
    public string fadeRendererChildName = "BookPage_Fade";

    [Header("Renderers (assign if you can)")]
    public SpriteRenderer baseRenderer;
    public SpriteRenderer fadeRenderer;

    [Header("Book Root (optional; for tooling that needs it)")]
    public Transform bigBookRoot;

    [Header("INTRO PAGE (Field Notes / Controls)")]
    public bool useIntroPage = true;
    public Sprite introPageSprite;

    [Header("Unlockable Spreads (Moonbow reveals ONLY)")]
    public Sprite blankSpreadSprite;
    public Sprite[] spreads;

    [Header("Crossfade")]
    public bool useCrossfade = true;
    public float crossfadeDuration = 0.22f;
    [Range(0f, 1f)] public float targetAlpha = 1f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip pageFlipClip;
    [Range(0f, 1f)] public float pageFlipVolume = 0.8f;
    public bool playFlipOnReveal = true;

    [Header("Behavior")]
    [Tooltip("When opening the book, never land on the blank end page. Open to the last non-blank page instead.")]
    public bool neverOpenToBlank = true;

    [Header("Debug")]
    public bool debugLogs = false;

    // State
    public bool IsOpen { get; private set; }

    // UnlockedCount counts ONLY unlockable spreads (spreads[]), NOT the intro page.
    public int UnlockedCount { get; private set; }  // 0..spreads.Length
    public int CurrentIndex { get; private set; }   // browsing index including intro + blank slot

    private HashSet<string> usedLocationIds = new HashSet<string>();

    private bool isTransitioning = false;
    private Coroutine transitionRoutine;

    // Save keys
    string KeyUnlocked => saveKeyPrefix + "UnlockedCount";
    string KeyCurrent  => saveKeyPrefix + "CurrentIndex";
    string KeyUsedIds  => saveKeyPrefix + "UsedLocationIds";

    int IntroCount => (useIntroPage && introPageSprite != null) ? 1 : 0;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureRenderers();
        EnsureAudio();

        Load();
        ClampIndex();
        HideFadeRendererImmediate();

        if (debugLogs)
            Debug.Log($"📖 Awake. IntroCount={IntroCount} Unlocked={UnlockedCount} Current={CurrentIndex} Max={GetMaxBrowsableIndex()} spreadsLen={(spreads != null ? spreads.Length : 0)}");
    }

    // NOTE: INPUT HAS BEEN REMOVED FROM THIS CLASS.
    // BookCarryToggleTiny should be the single place that handles arrow keys and calls NextPage()/PrevPage().

    // --------------------------
    // Called by BookCarryToggleTiny
    // --------------------------
    public void SetOpen(bool open)
    {
        IsOpen = open;

        if (IsOpen)
        {
            EnsureRenderers();
            ClampIndex();

            // Never open to the end blank page
            if (neverOpenToBlank && IsOnEndBlank(CurrentIndex))
            {
                int lastNonBlank = GetLastNonBlankIndex();
                CurrentIndex = Mathf.Clamp(lastNonBlank, 0, GetMaxBrowsableIndex());
                Save();
            }

            RenderCurrent(immediate: true);
        }
        else
        {
            HideFadeRendererImmediate();
        }

        if (debugLogs)
            Debug.Log($"📖 SetOpen({open}) IntroCount={IntroCount} Unlocked={UnlockedCount} Current={CurrentIndex} Max={GetMaxBrowsableIndex()} spreadsLen={(spreads != null ? spreads.Length : 0)}");
    }

    // --------------------------
    // Reveal API (called by mists)
    // --------------------------
    public bool RevealNextFromLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId)) return false;

        if (usedLocationIds.Contains(locationId))
        {
            if (debugLogs) Debug.Log($"📖 Reveal blocked (already used): {locationId}");
            return false;
        }

        usedLocationIds.Add(locationId);

        int spreadCount = (spreads != null) ? spreads.Length : 0;
        if (spreadCount <= 0)
        {
            Save();
            if (debugLogs) Debug.Log("📖 Reveal attempted but spreads[] is empty.");
            return false;
        }

        if (UnlockedCount >= spreadCount)
        {
            Save();
            if (debugLogs) Debug.Log("📖 Reveal attempted but already fully unlocked.");
            return false;
        }

        // Where the blank was BEFORE unlocking:
        int prevBlankIndex = IntroCount + UnlockedCount;
        bool playerWasOnBlank = (CurrentIndex == prevBlankIndex);

        // Unlock one spread
        UnlockedCount++;

        // If the player was looking at the blank when the reveal happened,
        // keep them on the same index (the blank "turns into" the new revealed spread).
        if (playerWasOnBlank)
        {
            CurrentIndex = prevBlankIndex;
        }

        ClampIndex();
        Save();

        if (IsOpen)
            RenderCurrent(immediate: !useCrossfade);

        if (playFlipOnReveal)
            PlayFlip();

        if (debugLogs)
            Debug.Log($"📖 Revealed Unlocked={UnlockedCount}/{spreadCount} CurrentIndex={CurrentIndex} prevBlank={prevBlankIndex} playerWasOnBlank={playerWasOnBlank}");

        return true;
    }

    // --------------------------
    // Paging (called by BookCarryToggleTiny)
    // --------------------------
    public void PrevPage()
    {
        ClampIndex();
        if (CurrentIndex <= 0) return;

        CurrentIndex--;
        ClampIndex();
        Save();

        RenderCurrent(immediate: !useCrossfade);
        PlayFlip();

        if (debugLogs) Debug.Log($"📖 Prev -> {CurrentIndex} sprite={GetSpriteName(CurrentIndex)}");
    }

    public void NextPage()
    {
        ClampIndex();
        int max = GetMaxBrowsableIndex();
        if (CurrentIndex >= max) return;

        CurrentIndex++;
        ClampIndex();
        Save();

        RenderCurrent(immediate: !useCrossfade);
        PlayFlip();

        if (debugLogs) Debug.Log($"📖 Next -> {CurrentIndex}/{max} sprite={GetSpriteName(CurrentIndex)}");
    }

    // --------------------------
    // Rendering
    // --------------------------
    void RenderCurrent(bool immediate)
    {
        EnsureRenderers();
        if (baseRenderer == null)
        {
            if (debugLogs) Debug.LogWarning("📖 baseRenderer is null. Assign 'Book blank' SpriteRenderer.");
            return;
        }

        Sprite targetSprite = GetSpriteForIndex(CurrentIndex);

        if (debugLogs)
            Debug.Log($"📖 Render idx={CurrentIndex} IntroCount={IntroCount} Unlocked={UnlockedCount} spreadsLen={(spreads != null ? spreads.Length : 0)} sprite={(targetSprite != null ? targetSprite.name : "(null)")}");

        if (fadeRenderer == null)
            immediate = true;

        if (immediate || !useCrossfade)
        {
            baseRenderer.sprite = targetSprite;
            Color bc = baseRenderer.color;
            bc.a = targetAlpha;
            baseRenderer.color = bc;

            HideFadeRendererImmediate();
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(CrossfadeTo(targetSprite));
    }

    IEnumerator CrossfadeTo(Sprite nextSprite)
    {
        if (baseRenderer == null || fadeRenderer == null)
        {
            isTransitioning = false;
            yield break;
        }

        isTransitioning = true;

        Color bc = baseRenderer.color;
        bc.a = targetAlpha;
        baseRenderer.color = bc;

        fadeRenderer.enabled = true;
        fadeRenderer.sprite = nextSprite;

        Color fc = fadeRenderer.color;
        fc.a = 0f;
        fadeRenderer.color = fc;

        float dur = Mathf.Max(0.01f, crossfadeDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);

            fc.a = Mathf.Lerp(0f, targetAlpha, k);
            fadeRenderer.color = fc;

            yield return null;
        }

        baseRenderer.sprite = nextSprite;

        HideFadeRendererImmediate();

        isTransitioning = false;
        transitionRoutine = null;
    }

    void HideFadeRendererImmediate()
    {
        if (fadeRenderer == null) return;

        fadeRenderer.enabled = false;
        fadeRenderer.sprite = null;

        Color fc = fadeRenderer.color;
        fc.a = 0f;
        fadeRenderer.color = fc;
    }

    Sprite GetSpriteForIndex(int index)
    {
        // Intro page at index 0 (if enabled)
        if (IntroCount == 1 && index == 0)
            return introPageSprite;

        // After intro, indices map to unlockables then blank.
        int realIndex = index - IntroCount; // 0.. unlockables/blank

        // If within unlocked unlockables -> show that spread
        if (spreads != null && realIndex >= 0 && realIndex < UnlockedCount && realIndex < spreads.Length)
            return spreads[realIndex];

        // Otherwise show the single blank end slot
        return blankSpreadSprite;
    }

    // --------------------------
    // One-blank-slot browsing rule
    // --------------------------
    int GetMaxBrowsableIndex()
    {
        int spreadCount = (spreads != null) ? spreads.Length : 0;

        int totalUnlockedPages = IntroCount + Mathf.Max(0, UnlockedCount);

        if (totalUnlockedPages <= 0)
            return 0;

        bool hasLockedRemaining = (spreadCount > 0 && UnlockedCount < spreadCount);

        // If more spreads remain locked, allow ONE blank at the end:
        // blank index == IntroCount + UnlockedCount
        if (hasLockedRemaining)
            return totalUnlockedPages;

        return Mathf.Max(0, totalUnlockedPages - 1);
    }

    bool IsOnEndBlank(int index)
    {
        int spreadCount = (spreads != null) ? spreads.Length : 0;
        if (spreadCount <= 0) return false;

        bool hasLockedRemaining = (UnlockedCount < spreadCount);
        if (!hasLockedRemaining) return false;

        int endBlankIndex = IntroCount + UnlockedCount;
        return index == endBlankIndex;
    }

    int GetLastNonBlankIndex()
    {
        int last = (IntroCount + UnlockedCount - 1);
        if (last < 0) last = 0;
        return last;
    }

    void ClampIndex()
    {
        int max = GetMaxBrowsableIndex();
        CurrentIndex = Mathf.Clamp(CurrentIndex, 0, max);
    }

    string GetSpriteName(int index)
    {
        Sprite s = GetSpriteForIndex(index);
        return (s != null ? s.name : "(null)");
    }

    // --------------------------
    // Save / Load
    // --------------------------
    void Save()
    {
        PlayerPrefs.SetInt(KeyUnlocked, UnlockedCount);
        PlayerPrefs.SetInt(KeyCurrent, CurrentIndex);

        string joined = string.Join("|", usedLocationIds);
        PlayerPrefs.SetString(KeyUsedIds, joined);

        PlayerPrefs.Save();
    }

    void Load()
    {
        UnlockedCount = PlayerPrefs.GetInt(KeyUnlocked, 0);
        CurrentIndex  = PlayerPrefs.GetInt(KeyCurrent, 0);

        if (spreads != null)
            UnlockedCount = Mathf.Clamp(UnlockedCount, 0, spreads.Length);
        else
            UnlockedCount = 0;

        usedLocationIds.Clear();
        string joined = PlayerPrefs.GetString(KeyUsedIds, "");
        if (!string.IsNullOrEmpty(joined))
        {
            string[] parts = joined.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                    usedLocationIds.Add(parts[i]);
            }
        }

        ClampIndex();
    }

    // --------------------------
    // Helpers
    // --------------------------
    void EnsureRenderers()
    {
        if (baseRenderer == null)
        {
            Transform t = FindDeepChildByName(transform, baseRendererChildName);
            if (t != null) baseRenderer = t.GetComponent<SpriteRenderer>();
        }

        if (fadeRenderer == null)
        {
            Transform t = FindDeepChildByName(transform, fadeRendererChildName);
            if (t != null) fadeRenderer = t.GetComponent<SpriteRenderer>();
        }
    }

    static Transform FindDeepChildByName(Transform root, string childName)
    {
        if (root == null) return null;

        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == childName)
                return all[i];

        return null;
    }

    void EnsureAudio()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void PlayFlip()
    {
        if (audioSource == null) return;
        if (pageFlipClip == null) return;

        audioSource.PlayOneShot(pageFlipClip, pageFlipVolume);
    }

    [ContextMenu("DEBUG: Reset Book Save")]
    public void DebugResetSave()
    {
        PlayerPrefs.DeleteKey(KeyUnlocked);
        PlayerPrefs.DeleteKey(KeyCurrent);
        PlayerPrefs.DeleteKey(KeyUsedIds);
        PlayerPrefs.Save();

        UnlockedCount = 0;
        CurrentIndex = 0;
        usedLocationIds.Clear();

        if (IsOpen)
            RenderCurrent(immediate: true);

        if (debugLogs)
            Debug.Log("📖 Book save reset.");
    }
}