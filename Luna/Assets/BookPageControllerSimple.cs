using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

public class BookControllerSimple : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Book UI")]
    public GameObject bookUIRoot;
    public Image bookPageImage;

    [Header("Reveal Transition")]
    public float revealDelay = 2f;
    public float revealFadeDuration = 0.5f;

    [Header("UI Fade")]
    public float uiFadeInDuration = 0.18f;
    public float uiFadeOutDuration = 0.18f;

    [Header("World Book Sync")]
    public bool syncToWorldOpenBook = true;
    public string openBookObjectName = "OpenBook";
    public float openBookRefreshInterval = 0.5f;
    public float uiOpenDebounce = 0.08f;
    public float uiCloseDebounce = 0.2f;
    public bool logOpenBookSearch = false;

    [Header("Input While Book Is Open")]
    public KeyCode previousPageKey = KeyCode.A;
    public KeyCode nextPageKey = KeyCode.D;
    public bool allowArrowKeysToo = true;

    [Header("Tap / Hold E Page Turning")]
    public KeyCode pageInteractKey = KeyCode.E;
    public float holdThreshold = 0.35f;
    public float pageTurnCooldown = 0.2f;

    [Header("Jump To Close")]
    public bool allowJumpToCloseBook = true;
    public string jumpButtonName = "Jump";
    public KeyCode jumpFallbackKey = KeyCode.Space;

    [Header("Optional Manual Close")]
    public bool allowManualCloseKey = false;
    public KeyCode manualCloseKey = KeyCode.Space;

    [Header("Pages")]
    public bool useIntroPage = true;
    public Sprite introPageSprite;
    public Sprite blankEndPageSprite;
    public Sprite[] unlockableSpreads;
    public bool neverOpenToBlank = false;
    private string currentOpenLocationId = "";


    [Header("Persistence")]
    public bool saveProgress = true;
    public string saveKeyPrefix = "BOOK_SIMPLE_";

    [Header("Optional Movement Lock")]
    public MonoBehaviour lunaMovementScript;
    public string horizontalLockBoolName = "MovementForbidden";

    [Header("Debug")]
    public bool debugLogs = false;

    public bool IsOpen => bookOpen;

    private int currentPage = 0;
    private bool bookOpen = false;
    private int unlockedCount = 0;
    private readonly HashSet<string> usedLocationIds = new HashSet<string>();

    private readonly List<OpenBookTrigger> openBookTriggers = new List<OpenBookTrigger>();
    private float nextOpenBookRefreshTime = 0f;

    private bool lastRawWorldOpen = false;
    private bool debouncedWorldOpen = false;
    private Coroutine syncRoutine;
    private Coroutine uiFadeRoutine;
    private Coroutine revealRoutine;

    private CanvasGroup bookCanvasGroup;
    private Image revealOverlayImage;

    private float ePressStartTime = 0f;
    private bool ePressedForBook = false;
    private bool holdActionTriggered = false;
    private float nextPageTurnTime = 0f;

    private string KeyUnlocked => saveKeyPrefix + "UnlockedCount";
    private string KeyIndex => saveKeyPrefix + "CurrentIndex";
    private string KeyUsedIds => saveKeyPrefix + "UsedLocationIds";

    void Awake()
    {
        if (saveProgress)
            Load();
    }

    void Start()
    {
        ResolvePlayer();
        SetupCanvasGroup();
        SetupRevealOverlay();

        if (bookUIRoot != null)
            bookUIRoot.SetActive(false);

        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 0f;
            bookCanvasGroup.interactable = false;
            bookCanvasGroup.blocksRaycasts = false;
        }

        ClampCurrentPage();

        if (syncToWorldOpenBook)
            RefreshOpenBookTriggers();
    }

    void Update()
    {
        if (player == null)
            ResolvePlayer();

        if (syncToWorldOpenBook)
        {
            if (Time.time >= nextOpenBookRefreshTime)
            {
                RefreshOpenBookTriggers();
                nextOpenBookRefreshTime = Time.time + Mathf.Max(0.05f, openBookRefreshInterval);
            }

            bool rawWorldOpen = AnyOpenBookActuallyOpen();

            if (rawWorldOpen != lastRawWorldOpen)
            {
                lastRawWorldOpen = rawWorldOpen;
                RestartDebouncedSync(rawWorldOpen);
            }
        }

        if (!bookOpen)
        {
            ResetBookInputState();
            return;
        }

        if (bookOpen && allowJumpToCloseBook && JumpPressedThisFrame())
        {
            bool closed = ForceCloseFirstOpenBook();
            ResetBookInputState();
            return;
        }

        if (allowManualCloseKey && Input.GetKeyDown(manualCloseKey))
        {
            CloseBookFromWorldSync();
            ResetBookInputState();
            return;
        }

        if (DialogueManager.isConversationActive)
        {
            ResetBookInputState();
            return;
        }

        HandleTapHoldBookPaging();
    }

    void SetupCanvasGroup()
    {
        if (bookUIRoot == null)
            return;

        bookCanvasGroup = bookUIRoot.GetComponent<CanvasGroup>();

        if (bookCanvasGroup == null)
            bookCanvasGroup = bookUIRoot.AddComponent<CanvasGroup>();
    }

    void SetupRevealOverlay()
    {
        if (bookPageImage == null)
            return;

        RectTransform baseRect = bookPageImage.rectTransform;
        Transform parent = baseRect.parent;
        if (parent == null)
            return;

        Transform existing = parent.Find(bookPageImage.name + "_RevealOverlay");
        if (existing != null)
            revealOverlayImage = existing.GetComponent<Image>();

        if (revealOverlayImage == null)
        {
            GameObject overlay = new GameObject(bookPageImage.name + "_RevealOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(parent, false);
            revealOverlayImage = overlay.GetComponent<Image>();
        }

        RectTransform overlayRect = revealOverlayImage.rectTransform;

        overlayRect.anchorMin = baseRect.anchorMin;
        overlayRect.anchorMax = baseRect.anchorMax;
        overlayRect.pivot = baseRect.pivot;
        overlayRect.anchoredPosition = baseRect.anchoredPosition;
        overlayRect.sizeDelta = baseRect.sizeDelta;
        overlayRect.localRotation = baseRect.localRotation;
        overlayRect.localScale = baseRect.localScale;
        overlayRect.offsetMin = baseRect.offsetMin;
        overlayRect.offsetMax = baseRect.offsetMax;

        revealOverlayImage.transform.SetAsLastSibling();
        revealOverlayImage.raycastTarget = false;
        revealOverlayImage.preserveAspect = bookPageImage.preserveAspect;
        revealOverlayImage.type = bookPageImage.type;
        revealOverlayImage.material = bookPageImage.material;
        revealOverlayImage.color = new Color(bookPageImage.color.r, bookPageImage.color.g, bookPageImage.color.b, 0f);
        revealOverlayImage.enabled = false;
    }

    void RestartDebouncedSync(bool targetOpen)
    {
        if (syncRoutine != null)
        {
            StopCoroutine(syncRoutine);
            syncRoutine = null;
        }

        syncRoutine = StartCoroutine(DebouncedSyncRoutine(targetOpen));
    }

    public void SetCurrentOpenLocation(string locationId)
    {
        currentOpenLocationId = locationId;
    }

    int GetMostRecentlyRevealedPageIndex()
    {
        if (unlockedCount <= 0)
            return 0;

        return GetIntroCount() + (unlockedCount - 1);
    }

    void ApplyOpenPageRuleForCurrentLocation()
    {
        bool hasBlankEndPage = blankEndPageSprite != null;
        bool hasCurrentLocation = !string.IsNullOrEmpty(currentOpenLocationId);
        bool locationAlreadyUsed = hasCurrentLocation && usedLocationIds.Contains(currentOpenLocationId);

        if (hasCurrentLocation && !locationAlreadyUsed && hasBlankEndPage)
        {
            // New lilystool: open to blank so the reveal can fade in beautifully.
            currentPage = GetPageCount() - 1;
        }
        else
        {
            // Revisited lilystool (or no location info): show most recently revealed spread.
            currentPage = GetMostRecentlyRevealedPageIndex();
        }

        ClampCurrentPage();
    }

    IEnumerator DebouncedSyncRoutine(bool targetOpen)
    {
        float waitTime = targetOpen ? uiOpenDebounce : uiCloseDebounce;

        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        bool currentRawWorldOpen = AnyOpenBookActuallyOpen();

        if (currentRawWorldOpen != targetOpen)
        {
            syncRoutine = null;
            yield break;
        }

        debouncedWorldOpen = targetOpen;

        if (!DialogueManager.isConversationActive)
        {
            if (debouncedWorldOpen && !bookOpen)
                OpenBookFromWorldSync();
            else if (!debouncedWorldOpen && bookOpen)
                CloseBookFromWorldSync();
        }

        syncRoutine = null;
    }

    void HandleTapHoldBookPaging()
    {
        if (Input.GetKeyDown(pageInteractKey))
        {
            ePressStartTime = Time.time;
            ePressedForBook = true;
            holdActionTriggered = false;
            SetHorizontalMovementLocked(true);
        }

        if (ePressedForBook && Input.GetKey(pageInteractKey))
        {
            SetHorizontalMovementLocked(true);

            float heldTime = Time.time - ePressStartTime;

            if (!holdActionTriggered && heldTime >= holdThreshold && Time.time >= nextPageTurnTime)
            {
                PreviousPage();
                nextPageTurnTime = Time.time + pageTurnCooldown;
                holdActionTriggered = true;
            }
        }

        if (ePressedForBook && Input.GetKeyUp(pageInteractKey))
        {
            float heldTime = Time.time - ePressStartTime;

            if (!holdActionTriggered && heldTime < holdThreshold && Time.time >= nextPageTurnTime)
            {
                NextPage();
                nextPageTurnTime = Time.time + pageTurnCooldown;
            }

            ResetBookInputState();
        }
    }

    void ResetBookInputState()
    {
        ePressedForBook = false;
        holdActionTriggered = false;
        SetHorizontalMovementLocked(false);
    }

    void OpenBookFromWorldSync()
    {
        if (bookUIRoot == null || bookPageImage == null)
            return;

        SetupCanvasGroup();
        SetupRevealOverlay();

        ClampCurrentPage();
        ApplyOpenPageRuleForCurrentLocation();

        bookOpen = true;
        bookUIRoot.SetActive(true);
        SetHorizontalMovementLocked(false);

        if (uiFadeRoutine != null)
        {
            StopCoroutine(uiFadeRoutine);
            uiFadeRoutine = null;
        }

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        Sprite newPage = GetCurrentPageSprite();

        bool hasCurrentLocation = !string.IsNullOrEmpty(currentOpenLocationId);
        bool locationAlreadyUsed = hasCurrentLocation && usedLocationIds.Contains(currentOpenLocationId);
        bool shouldOpenOnBlank = hasCurrentLocation && !locationAlreadyUsed && blankEndPageSprite != null;

        Sprite basePage = shouldOpenOnBlank ? blankEndPageSprite : newPage;

        bookPageImage.sprite = basePage;
        bookPageImage.enabled = basePage != null;

        if (revealOverlayImage != null)
        {
            revealOverlayImage.enabled = false;
            revealOverlayImage.sprite = null;

            Color c = revealOverlayImage.color;
            c.a = 0f;
            revealOverlayImage.color = c;
        }

        uiFadeRoutine = StartCoroutine(FadeUIRoutine(1f, uiFadeInDuration, true));

        if (newPage != null && newPage != basePage)
            revealRoutine = StartCoroutine(RevealPageRoutine(basePage, newPage));

        if (saveProgress)
            Save();
    }

    void CloseBookFromWorldSync()
    {
        bookOpen = false;
        ResetBookInputState();
        SetHorizontalMovementLocked(false);

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        if (revealOverlayImage != null)
        {
            revealOverlayImage.enabled = false;
            Color c = revealOverlayImage.color;
            c.a = 0f;
            revealOverlayImage.color = c;
        }

        if (uiFadeRoutine != null)
        {
            StopCoroutine(uiFadeRoutine);
            uiFadeRoutine = null;
        }

        uiFadeRoutine = StartCoroutine(FadeUIRoutine(0f, uiFadeOutDuration, false));

        if (saveProgress)
            Save();
    }

    IEnumerator FadeUIRoutine(float targetAlpha, float duration, bool keepActiveAtEnd)
    {
        if (bookUIRoot == null)
            yield break;

        SetupCanvasGroup();

        if (bookCanvasGroup == null)
            yield break;

        if (!bookUIRoot.activeSelf)
            bookUIRoot.SetActive(true);

        if (targetAlpha > 0f)
        {
            bookCanvasGroup.interactable = false;
            bookCanvasGroup.blocksRaycasts = false;
        }

        float startAlpha = bookCanvasGroup.alpha;

        if (duration <= 0f)
        {
            bookCanvasGroup.alpha = targetAlpha;
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                bookCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            bookCanvasGroup.alpha = targetAlpha;
        }

        if (keepActiveAtEnd)
        {
            bookCanvasGroup.interactable = true;
            bookCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            bookCanvasGroup.interactable = false;
            bookCanvasGroup.blocksRaycasts = false;
            bookUIRoot.SetActive(false);
        }

        uiFadeRoutine = null;
    }

    public void ForceRefreshOpenBooks()
    {
        RefreshOpenBookTriggers();
    }

    void RefreshOpenBookTriggers()
    {
        openBookTriggers.Clear();

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t == null)
                continue;

            if (t.name != openBookObjectName)
                continue;

            if (!t.gameObject.scene.IsValid())
                continue;

            if ((t.hideFlags & HideFlags.NotEditable) != 0 || (t.hideFlags & HideFlags.HideAndDontSave) != 0)
                continue;

            OpenBookTrigger trigger = t.GetComponent<OpenBookTrigger>();
            if (trigger == null)
                trigger = t.GetComponentInChildren<OpenBookTrigger>(true);

            if (trigger != null)
                openBookTriggers.Add(trigger);
        }
    }

    bool AnyOpenBookActuallyOpen()
    {
        for (int i = openBookTriggers.Count - 1; i >= 0; i--)
        {
            OpenBookTrigger trigger = openBookTriggers[i];

            if (trigger == null)
            {
                openBookTriggers.RemoveAt(i);
                continue;
            }

            if (!trigger.gameObject.scene.IsValid())
            {
                openBookTriggers.RemoveAt(i);
                continue;
            }

            if (trigger.IsOpen())
                return true;
        }

        return false;
    }

    bool JumpPressedThisFrame()
    {
        bool jumpPressed = false;

        if (!string.IsNullOrEmpty(jumpButtonName))
        {
            try
            {
                jumpPressed = Input.GetButtonDown(jumpButtonName);
            }
            catch
            {
            }
        }

        if (!jumpPressed)
            jumpPressed = Input.GetKeyDown(jumpFallbackKey);

        return jumpPressed;
    }

    bool ForceCloseFirstOpenBook()
    {
        for (int i = openBookTriggers.Count - 1; i >= 0; i--)
        {
            OpenBookTrigger trigger = openBookTriggers[i];

            if (trigger == null)
            {
                openBookTriggers.RemoveAt(i);
                continue;
            }

            if (!trigger.gameObject.scene.IsValid())
            {
                openBookTriggers.RemoveAt(i);
                continue;
            }

            if (!trigger.IsOpen())
                continue;

            trigger.ForceClose();
            return true;
        }

        return false;
    }

    public void NextPage()
    {
        int max = GetPageCount() - 1;
        if (max < 0) return;

        currentPage++;
        if (currentPage > max)
            currentPage = max;

        ShowPageImmediate();

        if (saveProgress)
            Save();
    }

    public void PreviousPage()
    {
        if (GetPageCount() <= 0) return;

        currentPage--;
        if (currentPage < 0)
            currentPage = 0;

        ShowPageImmediate();

        if (saveProgress)
            Save();
    }

    void ShowPageImmediate()
    {
        if (bookPageImage == null) return;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        Sprite page = GetCurrentPageSprite();
        bookPageImage.sprite = page;
        bookPageImage.enabled = page != null;

        if (revealOverlayImage != null)
        {
            revealOverlayImage.enabled = false;
            revealOverlayImage.sprite = null;

            Color c = revealOverlayImage.color;
            c.a = 0f;
            revealOverlayImage.color = c;
        }
    }

    IEnumerator RevealPageRoutine(Sprite oldPage, Sprite newPage)
    {
        SetupRevealOverlay();

        if (bookPageImage == null)
            yield break;

        if (revealOverlayImage == null)
        {
            ShowPageImmediate();
            revealRoutine = null;
            yield break;
        }

        bookPageImage.sprite = oldPage;
        bookPageImage.enabled = oldPage != null;

        revealOverlayImage.sprite = newPage;
        revealOverlayImage.enabled = newPage != null;

        RectTransform baseRect = bookPageImage.rectTransform;
        RectTransform overlayRect = revealOverlayImage.rectTransform;
        overlayRect.anchorMin = baseRect.anchorMin;
        overlayRect.anchorMax = baseRect.anchorMax;
        overlayRect.pivot = baseRect.pivot;
        overlayRect.anchoredPosition = baseRect.anchoredPosition;
        overlayRect.sizeDelta = baseRect.sizeDelta;
        overlayRect.localRotation = baseRect.localRotation;
        overlayRect.localScale = baseRect.localScale;
        overlayRect.offsetMin = baseRect.offsetMin;
        overlayRect.offsetMax = baseRect.offsetMax;

        Color overlayColor = revealOverlayImage.color;
        overlayColor.r = bookPageImage.color.r;
        overlayColor.g = bookPageImage.color.g;
        overlayColor.b = bookPageImage.color.b;
        overlayColor.a = 0f;
        revealOverlayImage.color = overlayColor;

        if (revealDelay > 0f)
            yield return new WaitForSeconds(revealDelay);

        float duration = Mathf.Max(0f, revealFadeDuration);

        if (duration <= 0f)
        {
            overlayColor.a = 1f;
            revealOverlayImage.color = overlayColor;
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                overlayColor = revealOverlayImage.color;
                overlayColor.a = t;
                revealOverlayImage.color = overlayColor;

                yield return null;
            }

            overlayColor = revealOverlayImage.color;
            overlayColor.a = 1f;
            revealOverlayImage.color = overlayColor;
        }

        bookPageImage.sprite = newPage;
        bookPageImage.enabled = newPage != null;

        revealOverlayImage.enabled = false;
        overlayColor = revealOverlayImage.color;
        overlayColor.a = 0f;
        revealOverlayImage.color = overlayColor;

        revealRoutine = null;
    }

    Sprite GetCurrentPageSprite()
    {
        int totalPages = GetPageCount();
        if (totalPages <= 0) return null;

        int introCount = GetIntroCount();
        int lastIndex = totalPages - 1;

        if (introCount == 1 && currentPage == 0)
            return introPageSprite;

        if (currentPage == lastIndex)
            return blankEndPageSprite;

        int spreadSlot = currentPage - introCount;
        if (spreadSlot >= 0 &&
            spreadSlot < unlockedCount &&
            unlockableSpreads != null &&
            spreadSlot < unlockableSpreads.Length)
        {
            return unlockableSpreads[spreadSlot];
        }

        return blankEndPageSprite;
    }

    int GetIntroCount()
    {
        return (useIntroPage && introPageSprite != null) ? 1 : 0;
    }

    int GetPageCount()
    {
        int introCount = GetIntroCount();
        int spreadCount = Mathf.Max(0, unlockedCount);
        int blankCount = blankEndPageSprite != null ? 1 : 0;

        return Mathf.Max(0, introCount + spreadCount + blankCount);
    }

    void ClampCurrentPage()
    {
        int max = GetPageCount() - 1;

        if (max < 0)
        {
            currentPage = 0;
            return;
        }

        currentPage = Mathf.Clamp(currentPage, 0, max);
    }

    bool IsOnBlankEndPage()
    {
        int lastIndex = GetPageCount() - 1;
        return blankEndPageSprite != null && currentPage == lastIndex;
    }

    int GetLastNonBlankIndex()
    {
        int lastNonBlank = GetPageCount() - 2;
        if (lastNonBlank < 0) lastNonBlank = 0;
        return lastNonBlank;
    }

    void ApplyNeverOpenToBlankRule()
    {
        if (!neverOpenToBlank) return;
        if (!IsOnBlankEndPage()) return;

        currentPage = GetLastNonBlankIndex();
        ClampCurrentPage();
    }

    public bool RevealNextFromLocation(string locationId)
    {
        if (string.IsNullOrEmpty(locationId)) return false;
        if (unlockableSpreads == null || unlockableSpreads.Length == 0) return false;
        if (usedLocationIds.Contains(locationId)) return false;
        if (unlockedCount >= unlockableSpreads.Length) return false;

        Sprite oldPage = GetCurrentPageSprite();

        usedLocationIds.Add(locationId);

        int previousLastIndex = GetPageCount() - 1;
        bool wasOnBlank = currentPage == previousLastIndex;

        unlockedCount = Mathf.Clamp(unlockedCount + 1, 0, unlockableSpreads.Length);

        if (wasOnBlank)
            currentPage = previousLastIndex;

        ClampCurrentPage();

        Sprite newPage = GetCurrentPageSprite();

        if (bookOpen)
        {
            if (revealRoutine != null)
            {
                StopCoroutine(revealRoutine);
                revealRoutine = null;
            }

            revealRoutine = StartCoroutine(RevealPageRoutine(oldPage, newPage));
        }
        else
        {
            ShowPageImmediate();
        }

        if (saveProgress)
            Save();

        return true;
    }

    void ResolvePlayer()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
    }

    void Save()
    {
        PlayerPrefs.SetInt(KeyUnlocked, unlockedCount);
        PlayerPrefs.SetInt(KeyIndex, currentPage);

        string joined = string.Join("|", usedLocationIds);
        PlayerPrefs.SetString(KeyUsedIds, joined);

        PlayerPrefs.Save();
    }

    void Load()
    {
        unlockedCount = PlayerPrefs.GetInt(KeyUnlocked, 0);
        currentPage = PlayerPrefs.GetInt(KeyIndex, 0);

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
            {
                if (!string.IsNullOrEmpty(parts[i]))
                    usedLocationIds.Add(parts[i]);
            }
        }

        ClampCurrentPage();
    }

    [ContextMenu("DEBUG: Reset Book Save")]
    public void DebugResetSave()
    {
        PlayerPrefs.DeleteKey(KeyUnlocked);
        PlayerPrefs.DeleteKey(KeyIndex);
        PlayerPrefs.DeleteKey(KeyUsedIds);
        PlayerPrefs.Save();

        unlockedCount = 0;
        currentPage = 0;
        usedLocationIds.Clear();

        if (bookOpen)
            ShowPageImmediate();
    }

    void SetHorizontalMovementLocked(bool locked)
    {
        if (lunaMovementScript == null) return;

        var type = lunaMovementScript.GetType();

        var field = type.GetField(horizontalLockBoolName);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(lunaMovementScript, locked);
            return;
        }

        var prop = type.GetProperty(horizontalLockBoolName);
        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
        {
            prop.SetValue(lunaMovementScript, locked, null);
            return;
        }
    }

    public bool HasUsedRevealId(string locationId)
    {
        return !string.IsNullOrEmpty(locationId) && usedLocationIds.Contains(locationId);
    }

    public bool IsShowingRevealableBlankPage()
    {
        if (!bookOpen) return false;
        if (blankEndPageSprite == null) return false;
        return IsOnBlankEndPage();
    }

    public bool IsBookOpen()
    {
        return bookOpen;
    }
}