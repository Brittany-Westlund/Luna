using UnityEngine;

public class BookFixOnLoad : MonoBehaviour
{
    [Header("Table Prop (in this scene)")]
    public GameObject closedBookTiny;     // the table prop book GO
    public Transform tablePoint;          // MaryBookTablePoint

    [Header("Destroy any carried/other book copies")]
    public bool destroyCarriedBookCopies = true;

    [Tooltip("If a GameObject name contains any of these, it will be destroyed (except the table prop).")]
    public string[] destroyNameContains =
    {
        "BookControllerSimple",
        "BookCarryToggle",
        "OpenBookTiny",
        "BookPageController",
        "BigBook",
        "Book blank"
    };

    [Header("Also destroy by component type names (optional)")]
    public bool destroyByComponentType = true;

    [Header("Debug")]
    public bool debugLogs = true;

    void Awake()
    {
        // 1) Force table prop on + positioned
        ForceTableBook();

        // 2) Kill any carried/persistent copies
        if (destroyCarriedBookCopies)
            DestroyOtherBooks();
    }

    void ForceTableBook()
    {
        if (closedBookTiny == null)
        {
            var found = GameObject.Find("ClosedBookTiny");
            if (found != null) closedBookTiny = found;
        }

        if (tablePoint == null)
        {
            var tp = GameObject.Find("MaryBookTablePoint");
            if (tp != null) tablePoint = tp.transform;
        }

        if (closedBookTiny == null)
        {
            Debug.LogError("📘 BookFixOnLoad: ClosedBookTiny not found/assigned.");
            return;
        }

        closedBookTiny.SetActive(true);

        // Re-enable sprite renderer in case something turned it off
        var sr = closedBookTiny.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        if (tablePoint != null)
        {
            closedBookTiny.transform.position = tablePoint.position;
            closedBookTiny.transform.rotation = tablePoint.rotation;
        }

        if (debugLogs) Debug.Log("📘 BookFixOnLoad: Forced table book active + positioned.");
    }

    void DestroyOtherBooks()
    {
        // Destroy by name match
        var allTransforms = FindObjectsOfType<Transform>(true);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            var t = allTransforms[i];
            if (t == null) continue;

            var go = t.gameObject;
            if (go == null) continue;

            if (closedBookTiny != null && go == closedBookTiny) continue;

            string n = go.name;

            for (int k = 0; k < destroyNameContains.Length; k++)
            {
                var key = destroyNameContains[k];
                if (!string.IsNullOrEmpty(key) && n.Contains(key))
                {
                    if (debugLogs) Debug.Log($"📘 BookFixOnLoad: Destroying book-ish GO by name: {n}");
                    Destroy(go);
                    break;
                }
            }
        }

        if (!destroyByComponentType) return;

        // Destroy if it has known book components (stronger than name matching)
        var bookController = FindObjectsOfType<BookControllerSimple>(true);
        for (int i = 0; i < bookController.Length; i++)
        {
            if (bookController[i] == null) continue;
            var go = bookController[i].gameObject;
            if (closedBookTiny != null && go == closedBookTiny) continue;

            if (debugLogs) Debug.Log($"📘 BookFixOnLoad: Destroying BookControllerSimple: {go.name}");
            Destroy(go);
        }
    }
}