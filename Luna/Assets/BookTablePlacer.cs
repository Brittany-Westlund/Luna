using UnityEngine;

public class BookForceSpawnAlways : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject closedBookTinyPrefab;   // your prefab that has BookControllerSimple on it
    public Transform tablePoint;              // MaryBookTablePoint

    [Header("Names inside the prefab")]
    public string openBookChildName = "OpenBookTiny"; // child that shows the open tiny sprite

    [Header("Debug")]
    public bool debugLogs = true;

    void Awake()
    {
        ForceSpawnNow();
    }

    // You can still call this from Dialogue System if you want
    public void MarkBookGiven()
    {
        ForceSpawnNow();
    }

    [ContextMenu("DEBUG: Force Spawn Now")]
    public void ForceSpawnNow()
    {
        if (closedBookTinyPrefab == null)
        {
            Debug.LogError("📘 BookForceSpawnAlways: closedBookTinyPrefab is NULL. Assign the prefab.");
            return;
        }

        if (tablePoint == null)
        {
            var tp = GameObject.Find("MaryBookTablePoint");
            if (tp != null) tablePoint = tp.transform;
        }

        // 1) Destroy any existing BookControllerSimple anywhere (including pinned-to-Luna ones)
        var existing = FindObjectsOfType<BookControllerSimple>(true);
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] == null) continue;
            if (debugLogs) Debug.Log($"📘 Destroying old BookControllerSimple: {existing[i].gameObject.name}");
            Destroy(existing[i].gameObject);
        }

        // 2) Spawn fresh on table
        Vector3 pos = (tablePoint != null) ? tablePoint.position : Vector3.zero;
        Quaternion rot = (tablePoint != null) ? tablePoint.rotation : Quaternion.identity;

        GameObject book = Instantiate(closedBookTinyPrefab, pos, rot);
        book.name = "ClosedBookTiny_RUNTIME";
        book.SetActive(true);

        // 3) FORCE CLOSED visible
        var closedSR = book.GetComponent<SpriteRenderer>();
        if (closedSR != null) closedSR.enabled = true;

        // 4) FORCE OPEN tiny hidden (so it doesn’t start “both on”)
        var openChild = FindDeepChild(book.transform, openBookChildName);
        if (openChild != null)
        {
            var openSR = openChild.GetComponent<SpriteRenderer>();
            if (openSR != null) openSR.enabled = false;
        }

        if (debugLogs) Debug.Log("📘 Spawned ClosedBookTiny_RUNTIME (closed on, open off).");
    }

    static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null) return null;
        var all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == childName)
                return all[i];
        return null;
    }
}