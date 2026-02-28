using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BookControllerSimple
/// - One script for: open/close, store/place, paging, reveal/unlock, saving.
/// - Page order: Intro -> UnlockedSpreads -> BlankEndPage
/// - Auto-store when far works even if book is open (it will close + store).
/// - Optional: never open to blank end page.
/// </summary>
[DisallowMultipleComponent]
public class BookControllerSimple : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";
    public string storePointName = "BookStorePoint";
    public string placedPointName = "BookPlacedPoint";

    [Header("Tiny Book Visuals (this object is the closed sprite)")]
    public SpriteRenderer tinyClosedRenderer;          // usually this object's SpriteRenderer
    public SpriteRenderer tinyOpenRenderer;            // OpenBookTiny SpriteRenderer (optional)

    [Header("Big Book Visuals (the page sprite renderer you show when open)")]
    public SpriteRenderer bigPageRenderer;             // the big book page SpriteRenderer to enable/disable

    [Header("Pages")]
    public bool useIntroPage = true;
    public Sprite introPageSprite;                     // Field Notes / Controls (first page)
    public Sprite blankEndPageSprite;                  // always last page
    public Sprite[] unlockableSpreads;                 // spreads revealed over time (inserted before blank)

    [Header("Open Behavior")]
    [Tooltip("When opening the book, never land on the blank end page. Open to the last non-blank page instead.")]
    public bool neverOpenToBlank = true;

    [Header("Persistence")]
    public bool saveProgress = true;
    public string saveKeyPrefix = "BOOK_SIMPLE_";

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.V;
    public float holdSecondsToStoreOrPlace = 1.0f;
    public KeyCode nextPageKey = KeyCode.UpArrow;
    public KeyCode prevPageKey = KeyCode.DownArrow;

    [Header("Movement (store/place)")]
    public float moveSpeed = 10f;
    public float snapDistance = 0.03f;
    public Vector3 fallbackStoreOffset = new Vector3(0f, 1.2f, 0f);
    public Vector3 fallbackPlacedOffset = new Vector3(0f, -0.2f, 0f);

    [Header("Auto Store When Far (closes even if open)")]
    public bool autoStoreWhenFar = true;
    public float autoStoreDistance = 3.5f;

    [Header("Sound (optional)")]
    public AudioSource audioSource;
    public AudioClip pageFlipClip;
    [Range(0f, 1f)] public float pageFlipVolume = 0.8f;
    public bool playFlipOnReveal = true;

    [Header("Debug")]
    public bool debugLogs = false;

    // --------------------
    // Runtime state
    // --------------------
    public bool IsOpen => isOpen;
    public bool IsStored => isStored;

    private Transform player;
    private Transform storePoint;
    private Transform placedPoint;

    private bool isOpen = false;
    private bool isStored = false;

    private bool vHeld = false;
    private float vHeldTime = 0f;
    private bool holdActionTriggered = false;

    private bool isMovingToTarget = false;
    private Vector3 moveTarget;
    private System.Action onArrive;

    // Page state
    // unlockedCount counts only unlockableSpreads[]
    private int unlockedCount = 0;
    private int currentIndex = 0; // index in the computed page list

    // Used location IDs so each mist location only reveals once
    private HashSet<string> usedLocationIds = new HashSet<string>();

    // Save keys
    private string KeyUnlocked => saveKeyPrefix + "UnlockedCount";
    private string KeyIndex => saveKeyPrefix + "CurrentIndex";
    private string KeyUsedIds => saveKeyPrefix + "UsedLocationIds";

    // --------------------
    // Unity lifecycle
    // --------------------
    void Awake()
    {
        if (tinyClosedRenderer == null)
            tinyClosedRenderer = GetComponent<SpriteRenderer>();

        FindPlayerAndPoints();
        EnsureAudio();

        if (saveProgress) Load();

        // Ensure initial visuals
        if (tinyOpenRenderer != null) tinyOpenRenderer.enabled = false;
        if (tinyClosedRenderer != null) tinyClosedRenderer.enabled = true;

        SetBigBookVisible(false);

        ClampIndexToPages();
        RenderCurrentPage(); // will render only if open (it isn't)
        if (debugLogs) Debug.Log($"📖 Awake | unlocked={unlockedCount} idx={currentIndex} pages={GetPageCount()} stored={isStored} open={isOpen}");
    }

    void Update()
    {
        if (player == null) FindPlayerAndPoints();
        else
        {
            if (storePoint == null) storePoint = FindDeepChildByName(player, storePointName);
            if (placedPoint == null) placedPoint = FindDeepChildByName(player, placedPointName);
        }

        // Auto-store check (even if open)
        if (autoStoreWhenFar)
            CheckAutoStoreDistance();

        HandleVInput();

        if (isOpen && !isStored && !isMovingToTarget)
            HandlePagingInput();

        if (isMovingToTarget)
            MoveTowardTarget();
    }

    // --------------------
    // Input
    // --------------------
    private void HandleVInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            vHeld = true;
            vHeldTime = 0f;
            holdActionTriggered = false;
        }

        if (vHeld && Input.GetKey(toggleKey))
        {
            vHeldTime += Time.deltaTime;

            if (!holdActionTriggered && vHeldTime >= holdSecondsToStoreOrPlace)
            {
                holdActionTriggered = true;

                if (!isStored)
                    BeginStoreToCap(forceCloseIfOpen: true);
                else
                    BeginPlaceFromCap(forceCloseIfOpen: true);
            }
        }

        if (Input.GetKeyUp(toggleKey))
        {
            vHeld = false;

            // Tap behavior
            if (!holdActionTriggered)
            {
                if (!isStored && !isMovingToTarget)
                    ToggleOpenClose();
            }
        }
    }

    private void HandlePagingInput()
    {
        if (Input.GetKeyDown(prevPageKey))
        {
            PrevPage();
        }

        if (Input.GetKeyDown(nextPageKey))
        {
            NextPage();
        }
    }

    // --------------------
    // Auto-store when far
    // --------------------
    private void CheckAutoStoreDistance()
    {
        if (player == null) return;
        if (isStored) return;
        if (isMovingToTarget) return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist >= autoStoreDistance)
        {
            // IMPORTANT: store even if open (force-close)
            BeginStoreToCap(forceCloseIfOpen: true);
        }
    }

    // --------------------
    // Open / Close
    // --------------------
    private void ToggleOpenClose()
    {
        // Only allow open/close if not stored
        if (isStored) return;

        isOpen = !isOpen;

        if (tinyClosedRenderer != null) tinyClosedRenderer.enabled = !isOpen;
        if (tinyOpenRenderer != null) tinyOpenRenderer.enabled = isOpen;

        SetBigBookVisible(isOpen);

        // When opening, render whatever page we last viewed (with optional rule)
        if (isOpen)
        {
            ClampIndexToPages();
            ApplyNeverOpenToBlankRule();
            RenderCurrentPage();
            Save();
        }

        if (debugLogs) Debug.Log($"📖 ToggleOpenClose -> {isOpen} | idx={currentIndex}/{GetPageCount() - 1}");
    }

    private void ForceClose()
    {
        if (!isOpen) return;

        isOpen = false;

        if (tinyOpenRenderer != null) tinyOpenRenderer.enabled = false;
        if (tinyClosedRenderer != null) tinyClosedRenderer.enabled = true;

        SetBigBookVisible(false);
    }

    private bool IsOnBlankEndPage()
    {
        int lastIndex = GetPageCount() - 1;
        return (blankEndPageSprite != null && lastIndex >= 0 && currentIndex == lastIndex);
    }

    private int GetLastNonBlankIndex()
    {
        // Non-blank pages are: intro (optional) + unlocked spreads.
        // Blank is always the last page.
        int lastNonBlank = GetPageCount() - 2; // just before blank
        if (lastNonBlank < 0) lastNonBlank = 0;
        return lastNonBlank;
    }

    private void ApplyNeverOpenToBlankRule()
    {
        if (!neverOpenToBlank) return;
        if (!IsOnBlankEndPage()) return;

        currentIndex = GetLastNonBlankIndex();
        ClampIndexToPages();
    }

    // --------------------
    // Store / Place
    // --------------------
    private void BeginStoreToCap(bool forceCloseIfOpen)
    {
        if (forceCloseIfOpen) ForceClose();

        Vector3 target = GetStoreTarget();
        BeginMoveTo(target, () =>
        {
            HideTinyRenderers();
            isStored = true;

            if (debugLogs) Debug.Log("📖 Stored.");
        });
    }

    private void BeginPlaceFromCap(bool forceCloseIfOpen)
    {
        if (forceCloseIfOpen) ForceClose();

        Vector3 target = GetPlacedTarget();
        BeginMoveTo(target, () =>
        {
            // When placed, show closed tiny book again
            if (tinyClosedRenderer != null) tinyClosedRenderer.enabled = true;
            if (tinyOpenRenderer != null) tinyOpenRenderer.enabled = false;

            isStored = false;

            if (debugLogs) Debug.Log("📖 Placed.");
        });
    }

    private void BeginMoveTo(Vector3 target, System.Action arriveAction)
    {
        isMovingToTarget = true;
        moveTarget = target;
        onArrive = arriveAction;
    }

    private void MoveTowardTarget()
    {
        transform.position = Vector3.Lerp(transform.position, moveTarget, Time.deltaTime * moveSpeed);

        if (Vector3.Distance(transform.position, moveTarget) <= snapDistance)
        {
            transform.position = moveTarget;
            isMovingToTarget = false;

            onArrive?.Invoke();
            onArrive = null;
        }
    }

    private Vector3 GetStoreTarget()
    {
        if (player == null) return transform.position;
        if (storePoint != null) return storePoint.position;
        return player.position + fallbackStoreOffset;
    }

    private Vector3 GetPlacedTarget()
    {
        if (player == null) return transform.position;
        if (placedPoint != null) return placedPoint.position;
        return player.position + fallbackPlacedOffset;
    }

    private void HideTinyRenderers()
    {
        if (tinyClosedRenderer != null) tinyClosedRenderer.enabled = false;
        if (tinyOpenRenderer != null) tinyOpenRenderer.enabled = false;
    }

    private void SetBigBookVisible(bool visible)
    {
        if (bigPageRenderer == null) return;
        bigPageRenderer.enabled = visible;
        if (!visible) bigPageRenderer.sprite = null;
    }

    // --------------------
    // Paging
    // --------------------
    public void PrevPage()
    {
        int max = GetPageCount() - 1;
        if (max < 0) return;
        if (currentIndex <= 0) return;

        currentIndex = Mathf.Clamp(currentIndex - 1, 0, max);
        RenderCurrentPage();
        PlayFlip();
        Save();

        if (debugLogs) Debug.Log($"📖 Prev -> idx={currentIndex} sprite={(GetCurrentSprite() ? GetCurrentSprite().name : "(null)")}");
    }

    public void NextPage()
    {
        int max = GetPageCount() - 1;
        if (max < 0) return;
        if (currentIndex >= max) return;

        currentIndex = Mathf.Clamp(currentIndex + 1, 0, max);
        RenderCurrentPage();
        PlayFlip();
        Save();

        if (debugLogs) Debug.Log($"📖 Next -> idx={currentIndex}/{max} sprite={(GetCurrentSprite() ? GetCurrentSprite().name : "(null)")}");
    }

    private void RenderCurrentPage()
    {
        if (bigPageRenderer == null) return;
        if (!isOpen) return;

        ClampIndexToPages();
        bigPageRenderer.sprite = GetCurrentSprite();
    }

    private Sprite GetCurrentSprite()
    {
        // Indexing:
        // [0] Intro (if enabled)
        // [1..unlockedCount] unlocked spreads
        // [last] blank end page
        int introCount = GetIntroCount();
        int totalPages = GetPageCount();
        int lastIndex = totalPages - 1;

        if (totalPages <= 0) return null;

        // Intro
        if (introCount == 1 && currentIndex == 0)
            return introPageSprite;

        // Blank (always last page)
        if (currentIndex == lastIndex)
            return blankEndPageSprite;

        // Unlocked spreads (inserted before blank)
        int spreadSlot = currentIndex - introCount; // 0..unlockedCount-1
        if (spreadSlot >= 0 && spreadSlot < unlockedCount && unlockableSpreads != null && spreadSlot < unlockableSpreads.Length)
            return unlockableSpreads[spreadSlot];

        return blankEndPageSprite;
    }

    private int GetIntroCount()
    {
        return (useIntroPage && introPageSprite != null) ? 1 : 0;
    }

    private int GetPageCount()
    {
        int introCount = GetIntroCount();
        int blankCount = (blankEndPageSprite != null) ? 1 : 0;
        int spreadsCount = Mathf.Max(0, unlockedCount);

        int total = introCount + spreadsCount + blankCount;
        return Mathf.Max(0, total);
    }

    private void ClampIndexToPages()
    {
        int max = GetPageCount() - 1;
        if (max < 0) { currentIndex = 0; return; }
        currentIndex = Mathf.Clamp(currentIndex, 0, max);
    }

    // --------------------
    // Reveal / Unlock API
    // --------------------
    public bool RevealNextFromLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId)) return false;
        if (unlockableSpreads == null || unlockableSpreads.Length == 0) return false;

        if (usedLocationIds.Contains(locationId))
        {
            if (debugLogs) Debug.Log($"📖 Reveal blocked (already used): {locationId}");
            return false;
        }

        if (unlockedCount >= unlockableSpreads.Length)
        {
            if (debugLogs) Debug.Log("📖 Reveal blocked (all spreads already unlocked).");
            return false;
        }

        usedLocationIds.Add(locationId);

        // If player was viewing the blank end page, keep them on the same index
        // so blank "turns into" the new spread and a new blank appears at the end.
        int prevLastIndex = GetPageCount() - 1;
        bool wasOnBlank = (currentIndex == prevLastIndex);

        unlockedCount = Mathf.Clamp(unlockedCount + 1, 0, unlockableSpreads.Length);

        if (wasOnBlank)
            currentIndex = prevLastIndex;

        ClampIndexToPages();

        if (isOpen)
            RenderCurrentPage();

        if (playFlipOnReveal)
            PlayFlip();

        Save();

        if (debugLogs) Debug.Log($"📖 Revealed! unlocked={unlockedCount}/{unlockableSpreads.Length} wasOnBlank={wasOnBlank} idx={currentIndex}");
        return true;
    }

    // --------------------
    // Save / Load
    // --------------------
    private void Save()
    {
        if (!saveProgress) return;

        PlayerPrefs.SetInt(KeyUnlocked, unlockedCount);
        PlayerPrefs.SetInt(KeyIndex, currentIndex);

        string joined = string.Join("|", usedLocationIds);
        PlayerPrefs.SetString(KeyUsedIds, joined);

        PlayerPrefs.Save();
    }

    private void Load()
    {
        unlockedCount = PlayerPrefs.GetInt(KeyUnlocked, 0);
        currentIndex = PlayerPrefs.GetInt(KeyIndex, 0);

        if (unlockableSpreads != null)
            unlockedCount = Mathf.Clamp(unlockedCount, 0, unlockableSpreads.Length);
        else
            unlockedCount = 0;

        usedLocationIds.Clear();
        string joined = PlayerPrefs.GetString(KeyUsedIds, "");
        if (!string.IsNullOrEmpty(joined))
        {
            string[] parts = joined.Split('|');
            for (int i = 0; i < parts.Length; i++)
                if (!string.IsNullOrEmpty(parts[i]))
                    usedLocationIds.Add(parts[i]);
        }

        ClampIndexToPages();
    }

    [ContextMenu("DEBUG: Reset Book Save")]
    public void DebugResetSave()
    {
        PlayerPrefs.DeleteKey(KeyUnlocked);
        PlayerPrefs.DeleteKey(KeyIndex);
        PlayerPrefs.DeleteKey(KeyUsedIds);
        PlayerPrefs.Save();

        unlockedCount = 0;
        currentIndex = 0;
        usedLocationIds.Clear();

        if (isOpen)
            RenderCurrentPage();

        if (debugLogs) Debug.Log("📖 Reset save.");
    }

    // --------------------
    // Helpers
    // --------------------
    private void FindPlayerAndPoints()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p == null) return;

        player = p.transform;
        storePoint = FindDeepChildByName(player, storePointName);
        placedPoint = FindDeepChildByName(player, placedPointName);
    }

    private static Transform FindDeepChildByName(Transform parent, string childName)
    {
        if (parent == null) return null;

        Transform[] all = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == childName)
                return all[i];

        return null;
    }

    private void EnsureAudio()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void PlayFlip()
    {
        if (audioSource == null) return;
        if (pageFlipClip == null) return;

        audioSource.PlayOneShot(pageFlipClip, pageFlipVolume);
    }
}