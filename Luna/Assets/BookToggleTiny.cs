// BookCarryToggleTiny.cs
// Behavior:
// - Tap V toggles open/close
// - Hold V stores/places
// - When opened, it shows the previously viewed page (no forced jump to blank)
// - Arrow keys handled here so they always work.

using UnityEngine;

public class BookCarryToggleTiny : MonoBehaviour
{
    [Header("Big Book Root (PARENT that contains the page renderer objects)")]
    public GameObject bigBookRoot;

    [Header("Big Book Page Renderer Object (SpriteRenderer you show/hide)")]
    public GameObject bigBookPageObject;

    [Header("Child Open Book (OpenBookTiny)")]
    public GameObject openBookTiny;

    [Header("Player Lookup")]
    public string playerTag = "Player";

    [Header("Deep Child Point Names (type these)")]
    public string storePointName = "BookStorePoint";
    public string placedPointName = "BookPlacedPoint";

    [Header("Hold Settings")]
    public float holdSecondsToStoreOrPlace = 1.0f;

    [Header("Move Settings")]
    public float moveSpeed = 10f;
    public float snapDistance = 0.03f;

    [Header("Fallback Offsets (if points not found)")]
    public Vector3 fallbackStoreOffset = new Vector3(0f, 1.2f, 0f);
    public Vector3 fallbackPlacedOffset = new Vector3(0f, -0.2f, 0f);

    [Header("Optional: Auto-store when player walks away")]
    public bool autoStoreWhenFar = false;
    public float autoStoreDistance = 3.5f;

    [Header("Optional: Force Big Book Sorting")]
    public bool forceBigBookSorting = true;
    public string bigBookSortingLayerName = "UI";
    public int bigBookOrderInLayer = 200;

    [Header("Debug")]
    public bool debugLogs = false;

    private Transform player;
    private Transform storePoint;
    private Transform placedPoint;

    private SpriteRenderer closedRenderer;
    private SpriteRenderer openRenderer;

    private SpriteRenderer bigPageRenderer;

    private bool isOpen = false;
    private bool isStored = false;

    private bool vHeld = false;
    private float vHeldTime = 0f;
    private bool holdActionTriggered = false;

    private bool isMovingToTarget = false;
    private Vector3 moveTarget;
    private System.Action onArrive;

    private BookPageController pageController;

    void Awake()
    {
        closedRenderer = GetComponent<SpriteRenderer>();

        if (openBookTiny != null)
            openRenderer = openBookTiny.GetComponent<SpriteRenderer>();

        if (bigBookPageObject != null)
            bigPageRenderer = bigBookPageObject.GetComponent<SpriteRenderer>();

        FindPlayerAndPoints();
        ResolvePageController();
    }

    void Start()
    {
        if (openRenderer != null) openRenderer.enabled = false;
        if (closedRenderer != null) closedRenderer.enabled = true;

        SetBigBookVisible(false);

        if (forceBigBookSorting && bigPageRenderer != null)
        {
            if (!string.IsNullOrEmpty(bigBookSortingLayerName))
                bigPageRenderer.sortingLayerName = bigBookSortingLayerName;
            bigPageRenderer.sortingOrder = bigBookOrderInLayer;
        }

        ResolvePageController();
        PushBigRootIntoController();
        if (pageController != null)
            pageController.SetOpen(false);
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayerAndPoints();
        }
        else
        {
            if (storePoint == null) storePoint = FindDeepChildByName(player, storePointName);
            if (placedPoint == null) placedPoint = FindDeepChildByName(player, placedPointName);
        }

        ResolvePageController();
        PushBigRootIntoController();

        HandleVInput();
        HandleArrowPagingInput();

        if (isMovingToTarget)
            MoveTowardTarget();

        if (autoStoreWhenFar)
            CheckAutoStoreDistance();
    }

    void ResolvePageController()
    {
        if (BookPageController.Instance != null)
        {
            pageController = BookPageController.Instance;
            return;
        }

        if (pageController == null)
            pageController = GetComponent<BookPageController>();

        if (pageController == null)
        {
            var all = Resources.FindObjectsOfTypeAll<BookPageController>();
            if (all != null && all.Length > 0)
                pageController = all[0];
        }
    }

    void PushBigRootIntoController()
    {
        if (pageController == null) return;

        if (bigBookRoot != null)
            pageController.bigBookRoot = bigBookRoot.transform;
        else if (bigBookPageObject != null)
            pageController.bigBookRoot = bigBookPageObject.transform;
    }

    void HandleArrowPagingInput()
    {
        if (!isOpen) return;
        if (isStored) return;
        if (isMovingToTarget) return;
        if (pageController == null) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (debugLogs) Debug.Log("📖 DownArrow -> PrevPage()");
            pageController.PrevPage();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (debugLogs) Debug.Log("📖 UpArrow -> NextPage()");
            pageController.NextPage();
        }
    }

    void CheckAutoStoreDistance()
    {
        if (player == null) return;
        if (isStored) return;
        if (isOpen) return;
        if (isMovingToTarget) return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist >= autoStoreDistance)
            BeginStoreToCap();
    }

    void HandleVInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            vHeld = true;
            vHeldTime = 0f;
            holdActionTriggered = false;
        }

        if (vHeld && Input.GetKey(KeyCode.V))
        {
            vHeldTime += Time.deltaTime;

            if (!holdActionTriggered && vHeldTime >= holdSecondsToStoreOrPlace)
            {
                holdActionTriggered = true;

                if (!isStored)
                    BeginStoreToCap();
                else
                    BeginPlaceFromCap();
            }
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            vHeld = false;

            if (!holdActionTriggered)
            {
                if (!isStored && !isMovingToTarget)
                    ToggleOpenClose();
            }
        }
    }

    void ToggleOpenClose()
    {
        isOpen = !isOpen;

        SetBigBookVisible(isOpen);

        if (closedRenderer != null) closedRenderer.enabled = !isOpen;
        if (openRenderer != null) openRenderer.enabled = isOpen;

        ResolvePageController();
        PushBigRootIntoController();

        if (pageController != null)
        {
            // IMPORTANT CHANGE: do NOT jump to blank when opening.
            // It will render CurrentIndex (last viewed).
            pageController.SetOpen(isOpen);
        }

        if (debugLogs)
            Debug.Log($"📖 ToggleOpenClose -> {isOpen} (controller={(pageController != null ? "OK" : "NULL")})");
    }

    void BeginStoreToCap()
    {
        if (isOpen)
        {
            isOpen = false;
            SetBigBookVisible(false);

            ResolvePageController();
            if (pageController != null) pageController.SetOpen(false);
        }

        if (openRenderer != null) openRenderer.enabled = false;
        if (closedRenderer != null) closedRenderer.enabled = true;

        Vector3 target = GetStoreTarget();
        BeginMoveTo(target, () =>
        {
            HideTinyRenderers();
            isStored = true;
        });
    }

    void BeginPlaceFromCap()
    {
        if (isOpen)
        {
            isOpen = false;
            SetBigBookVisible(false);

            ResolvePageController();
            if (pageController != null) pageController.SetOpen(false);
        }

        Vector3 target = GetPlacedTarget();
        BeginMoveTo(target, () =>
        {
            if (openRenderer != null) openRenderer.enabled = false;
            if (closedRenderer != null) closedRenderer.enabled = true;

            isStored = false;
        });
    }

    void BeginMoveTo(Vector3 target, System.Action arriveAction)
    {
        isMovingToTarget = true;
        moveTarget = target;
        onArrive = arriveAction;
    }

    void MoveTowardTarget()
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

    Vector3 GetStoreTarget()
    {
        if (player == null) return transform.position;
        if (storePoint != null) return storePoint.position;
        return player.position + fallbackStoreOffset;
    }

    Vector3 GetPlacedTarget()
    {
        if (player == null) return transform.position;
        if (placedPoint != null) return placedPoint.position;
        return player.position + fallbackPlacedOffset;
    }

    void HideTinyRenderers()
    {
        if (closedRenderer != null) closedRenderer.enabled = false;
        if (openRenderer != null) openRenderer.enabled = false;
    }

    void SetBigBookVisible(bool visible)
    {
        if (bigBookPageObject == null) return;

        if (bigPageRenderer == null)
            bigPageRenderer = bigBookPageObject.GetComponent<SpriteRenderer>();

        if (bigPageRenderer != null)
            bigPageRenderer.enabled = visible;
    }

    void FindPlayerAndPoints()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p == null) return;

        player = p.transform;

        storePoint = FindDeepChildByName(player, storePointName);
        placedPoint = FindDeepChildByName(player, placedPointName);
    }

    static Transform FindDeepChildByName(Transform parent, string childName)
    {
        if (parent == null) return null;

        Transform[] all = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == childName)
                return all[i];
        return null;
    }
}