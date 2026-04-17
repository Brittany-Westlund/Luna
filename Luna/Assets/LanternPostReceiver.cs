using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LanternPostReceiver : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";
    public string requiredObjectName = "AcornLantern";

    [Header("Player Hold Point")]
    [Tooltip("Assign Luna's AcornHoldPoint here for reliable detection.")]
    public Transform playerHoldPoint;

    [Header("Lantern Visual")]
    [Tooltip("The Lantern child to turn ON.")]
    public GameObject lanternObject;

    [Header("Options")]
    public bool destroyHeldObject = true;
    public bool disableHeldObject = false;
    public bool triggerOnce = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool hasActivated = false;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Awake()
    {
        if (lanternObject != null)
            lanternObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (triggerOnce && hasActivated)
            return;

        GameObject heldObject = FindHeldLantern(other.transform);

        if (heldObject == null)
        {
            if (debugLogs)
                Debug.Log($"[LanternPostReceiver] No AcornLantern found on player.");
            return;
        }

        if (debugLogs)
            Debug.Log($"[LanternPostReceiver] Received {heldObject.name}");

        // Remove held lantern
        if (destroyHeldObject)
        {
            Destroy(heldObject);
        }
        else if (disableHeldObject)
        {
            heldObject.SetActive(false);
        }

        // Turn on lantern
        if (lanternObject != null)
            lanternObject.SetActive(true);

        hasActivated = true;
    }

    private GameObject FindHeldLantern(Transform playerRoot)
    {
        if (playerHoldPoint != null)
        {
            for (int i = 0; i < playerHoldPoint.childCount; i++)
            {
                Transform child = playerHoldPoint.GetChild(i);
                if (child.name.Contains(requiredObjectName))
                    return child.gameObject;
            }
        }

        // fallback search
        Transform[] allChildren = playerRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allChildren)
        {
            if (t.name.Contains(requiredObjectName) && t.gameObject.activeInHierarchy)
                return t.gameObject;
        }

        return null;
    }
}