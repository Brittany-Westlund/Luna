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

    [Header("Lilystool Detection")]
    public float lilystoolRadius = 2f;
    public string lilystoolTag = "Lilystool";

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.V;
    public KeyCode previousPageKey = KeyCode.A;
    public KeyCode nextPageKey = KeyCode.D;
    public bool allowArrowKeysToo = true;

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

        if (Input.GetKeyDown(toggleKey))
        {
            if (bookOpen)
            {
                CloseBook();
            }
            else if (NearLilystool())
            {
                OpenBook();
            }
        }

        if (!bookOpen) return;

        bool prevPressed = Input.GetKeyDown(previousPageKey) || (allowArrowKeysToo && Input.GetKeyDown(KeyCode.LeftArrow));
        bool nextPressed = Input.GetKeyDown(nextPageKey) || (allowArrowKeysToo && Input.GetKeyDown(KeyCode.RightArrow));

        if (prevPressed)
            PreviousPage();

        if (nextPressed)
            NextPage();
    }

    void OpenBook()
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
            Debug.Log($"📖 OpenBook -> currentPage={currentPage}");
    }

    void CloseBook()
    {
        bookOpen = false;

        if (bookUIRoot != null)
            bookUIRoot.SetActive(false);

        SetHorizontalMovementLocked(false);

        if (debugLogs)
            Debug.Log("📖 CloseBook");
    }

    void NextPage()
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

    void PreviousPage()
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

    bool NearLilystool()
    {
        if (player == null) return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, lilystoolRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag(lilystoolTag))
                return true;
        }

        return false;
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

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(player.position, lilystoolRadius);
    }
}
