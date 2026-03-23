using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BookControllerSimple : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public string playerTag = "Player";

    [Header("Book UI")]
    public GameObject bookUIRoot;
    public Image bookPageImage;

    [Header("World Book Sync")]
    [Tooltip("If true, MoonbowBookUI mirrors the state of any world OpenBook sprite renderer.")]
    public bool syncToWorldOpenBook = true;

    [Tooltip("Exact GameObject name to search for in the scene.")]
    public string openBookObjectName = "OpenBook";

    [Tooltip("How often to refresh the list of OpenBook renderers.")]
    public float openBookRefreshInterval = 0.5f;

    [Tooltip("If true, logs which OpenBook objects were found and when sync changes.")]
    public bool logOpenBookSearch = false;

    [Header("Input While Book Is Open")]
    public KeyCode previousPageKey = KeyCode.A;
    public KeyCode nextPageKey = KeyCode.D;
    public bool allowArrowKeysToo = true;

    [Header("Jump To Close")]
    [Tooltip("If true, pressing Jump while the book is open will turn off the visible world OpenBook.")]
    public bool allowJumpToCloseBook = true;

    [Tooltip("Uses Unity's default Jump button name.")]
    public string jumpButtonName = "Jump";

    [Tooltip("Optional fallback key if Input.GetButtonDown(\"Jump\") is not set up the way you want.")]
    public KeyCode jumpFallbackKey = KeyCode.Space;

    [Tooltip("If true, disables the whole OpenBook GameObject instead of only its SpriteRenderer.")]
    public bool jumpCloseDisablesWholeObject = false;

    [Header("Optional Manual Close")]
    [Tooltip("Optional fallback close key. Usually leave off if world book should be the sole authority.")]
    public bool allowManualCloseKey = false;
    public KeyCode manualCloseKey = KeyCode.Space;

    [Header("Pages")]
    public bool useIntroPage = true;
    public Sprite introPageSprite;
    public Sprite blankEndPageSprite;
    public Sprite[] unlockableSpreads;
    public bool neverOpenToBlank = true;

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

    private readonly List<SpriteRenderer> openBookRenderers = new List<SpriteRenderer>();
    private float nextOpenBookRefreshTime = 0f;
    private bool lastDetectedWorldBookVisible = false;

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

        if (bookUIRoot != null)
            bookUIRoot.SetActive(false);

        ClampCurrentPage();

        if (syncToWorldOpenBook)
            RefreshOpenBookRenderers();

        if (debugLogs)
        {
            Debug.Log($"📖 BookControllerSimple Start");
            Debug.Log($"📖 player = {(player != null ? player.name : "NULL")}");
            Debug.Log($"📖 bookUIRoot = {(bookUIRoot != null ? bookUIRoot.name : "NULL")}");
            Debug.Log($"📖 bookPageImage = {(bookPageImage != null ? bookPageImage.name : "NULL")}");
            Debug.Log($"📖 unlockedCount = {unlockedCount}");
            Debug.Log($"📖 currentPage = {currentPage}");
        }
    }

    void Update()
    {
        if (player == null)
            ResolvePlayer();

        if (syncToWorldOpenBook)
        {
            if (Time.time >= nextOpenBookRefreshTime)
            {
                RefreshOpenBookRenderers();
                nextOpenBookRefreshTime = Time.time + Mathf.Max(0.05f, openBookRefreshInterval);
            }

            bool shouldBeOpen = AnyOpenBookVisible();

            if (shouldBeOpen != lastDetectedWorldBookVisible)
            {
                lastDetectedWorldBookVisible = shouldBeOpen;

                if (debugLogs || logOpenBookSearch)
                    Debug.Log($"📖 World OpenBook visible changed -> {shouldBeOpen}");
            }

            if (shouldBeOpen && !bookOpen)
            {
                OpenBookFromWorldSync();
            }
            else if (!shouldBeOpen && bookOpen)
            {
                CloseBookFromWorldSync();
            }
        }

        if (!bookOpen)
            return;

        if (bookOpen && allowJumpToCloseBook && JumpPressedThisFrame())
        {
            bool turnedOff = TurnOffVisibleOpenBook();

            if (debugLogs)
                Debug.Log($"📖 Jump close attempted -> turnedOff={turnedOff}");

            return;
        }

        if (allowManualCloseKey && Input.GetKeyDown(manualCloseKey))
        {
            CloseBookFromWorldSync();
            return;
        }

        bool prevPressed = Input.GetKeyDown(previousPageKey) || (allowArrowKeysToo && Input.GetKeyDown(KeyCode.LeftArrow));
        bool nextPressed = Input.GetKeyDown(nextPageKey) || (allowArrowKeysToo && Input.GetKeyDown(KeyCode.RightArrow));

        if (prevPressed)
            PreviousPage();

        if (nextPressed)
            NextPage();
    }

    void OpenBookFromWorldSync()
    {
        if (bookUIRoot == null || bookPageImage == null)
        {
            Debug.LogWarning("BookControllerSimple: bookUIRoot or bookPageImage is missing.");
            return;
        }

        ClampCurrentPage();
        ApplyNeverOpenToBlankRule();

        bookOpen = true;
        bookUIRoot.SetActive(true);

        ShowPage();
        SetHorizontalMovementLocked(true);

        if (saveProgress)
            Save();

        if (debugLogs)
            Debug.Log($"📖 OpenBookFromWorldSync -> currentPage={currentPage}");
    }

    void CloseBookFromWorldSync()
    {
        bookOpen = false;

        if (bookUIRoot != null)
            bookUIRoot.SetActive(false);

        SetHorizontalMovementLocked(false);

        if (saveProgress)
            Save();

        if (debugLogs)
            Debug.Log("📖 CloseBookFromWorldSync");
    }

    public void ForceRefreshOpenBooks()
    {
        RefreshOpenBookRenderers();
    }

    void RefreshOpenBookRenderers()
    {
        openBookRenderers.Clear();

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

            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = t.GetComponentInChildren<SpriteRenderer>(true);

            if (sr != null)
            {
                openBookRenderers.Add(sr);

                if (logOpenBookSearch)
                    Debug.Log($"📖 Found OpenBook renderer on: {t.gameObject.name} (path: {GetHierarchyPath(t)})");
            }
            else if (logOpenBookSearch)
            {
                Debug.LogWarning($"📖 Found object named '{openBookObjectName}' but no SpriteRenderer on it or its children: {GetHierarchyPath(t)}");
            }
        }

        if (logOpenBookSearch)
            Debug.Log($"📖 RefreshOpenBookRenderers complete. Found {openBookRenderers.Count} matching renderer(s).");
    }

    bool AnyOpenBookVisible()
    {
        for (int i = openBookRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer sr = openBookRenderers[i];

            if (sr == null)
            {
                openBookRenderers.RemoveAt(i);
                continue;
            }

            if (!sr.gameObject.scene.IsValid())
            {
                openBookRenderers.RemoveAt(i);
                continue;
            }

            if (IsSpriteRendererActuallyVisible(sr))
                return true;
        }

        return false;
    }

    bool IsSpriteRendererActuallyVisible(SpriteRenderer sr)
    {
        if (sr == null)
            return false;

        if (!sr.enabled)
            return false;

        if (!sr.gameObject.activeInHierarchy)
            return false;

        Color c = sr.color;
        if (c.a <= 0.001f)
            return false;

        return true;
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
                // Ignore if Jump button is not configured in Input Manager
            }
        }

        if (!jumpPressed)
            jumpPressed = Input.GetKeyDown(jumpFallbackKey);

        return jumpPressed;
    }

    bool TurnOffVisibleOpenBook()
    {
        for (int i = openBookRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer sr = openBookRenderers[i];

            if (sr == null)
            {
                openBookRenderers.RemoveAt(i);
                continue;
            }

            if (!sr.gameObject.scene.IsValid())
            {
                openBookRenderers.RemoveAt(i);
                continue;
            }

            if (!IsSpriteRendererActuallyVisible(sr))
                continue;

            if (jumpCloseDisablesWholeObject)
            {
                sr.gameObject.SetActive(false);

                if (debugLogs)
                    Debug.Log($"📖 Jump closed OpenBook by disabling GameObject: {sr.gameObject.name}");
            }
            else
            {
                sr.enabled = false;

                if (debugLogs)
                    Debug.Log($"📖 Jump closed OpenBook by disabling SpriteRenderer: {sr.gameObject.name}");
            }

            return true;
        }

        return false;
    }

    string GetHierarchyPath(Transform t)
    {
        if (t == null)
            return "NULL";

        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }

    public void NextPage()
    {
        int max = GetPageCount() - 1;
        if (max < 0) return;

        currentPage++;
        if (currentPage > max)
            currentPage = max;

        ShowPage();

        if (saveProgress)
            Save();

        if (debugLogs)
            Debug.Log($"📖 NextPage -> currentPage={currentPage}");
    }

    public void PreviousPage()
    {
        if (GetPageCount() <= 0) return;

        currentPage--;
        if (currentPage < 0)
            currentPage = 0;

        ShowPage();

        if (saveProgress)
            Save();

        if (debugLogs)
            Debug.Log($"📖 PreviousPage -> currentPage={currentPage}");
    }

    void ShowPage()
    {
        if (bookPageImage == null) return;

        Sprite page = GetCurrentPageSprite();
        bookPageImage.sprite = page;
        bookPageImage.enabled = page != null;

        if (debugLogs)
            Debug.Log($"📖 ShowPage -> {(page != null ? page.name : "NULL")}");
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

        if (usedLocationIds.Contains(locationId))
        {
            if (debugLogs)
                Debug.Log($"📖 Reveal blocked; already used {locationId}");
            return false;
        }

        if (unlockedCount >= unlockableSpreads.Length)
        {
            if (debugLogs)
                Debug.Log("📖 Reveal blocked; all spreads unlocked");
            return false;
        }

        usedLocationIds.Add(locationId);

        int previousLastIndex = GetPageCount() - 1;
        bool wasOnBlank = currentPage == previousLastIndex;

        unlockedCount = Mathf.Clamp(unlockedCount + 1, 0, unlockableSpreads.Length);

        if (wasOnBlank)
            currentPage = previousLastIndex;

        ClampCurrentPage();

        if (bookOpen)
            ShowPage();

        if (saveProgress)
            Save();

        if (debugLogs)
            Debug.Log($"📖 Reveal success -> unlockedCount={unlockedCount}");

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
            ShowPage();
    }

    void SetHorizontalMovementLocked(bool locked)
    {
        if (lunaMovementScript == null) return;

        var type = lunaMovementScript.GetType();

        var field = type.GetField(horizontalLockBoolName);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(lunaMovementScript, locked);

            if (debugLogs)
                Debug.Log($"📖 Set field {horizontalLockBoolName} = {locked}");

            return;
        }

        var prop = type.GetProperty(horizontalLockBoolName);
        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
        {
            prop.SetValue(lunaMovementScript, locked, null);

            if (debugLogs)
                Debug.Log($"📖 Set property {horizontalLockBoolName} = {locked}");

            return;
        }

        Debug.LogWarning($"BookControllerSimple: Could not find bool field/property '{horizontalLockBoolName}' on {lunaMovementScript.GetType().Name}");
    }

    public bool HasUsedRevealId(string locationId)
    {
        return !string.IsNullOrEmpty(locationId) && usedLocationIds.Contains(locationId);
    }

    public bool IsShowingRevealableBlankPage()
    {
        if (!bookOpen)
            return false;

        if (blankEndPageSprite == null)
            return false;

        return IsOnBlankEndPage();
    }

    public bool IsBookOpen()
    {
        return bookOpen;
    }
}