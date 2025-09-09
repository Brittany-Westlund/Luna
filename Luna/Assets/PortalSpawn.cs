using UnityEngine;

public class PortalSpawn : MonoBehaviour
{
    [Tooltip("Must match LevelPortal.portalID from the portal Luna used.")]
    public string portalID;

    [Tooltip("Child Transform under THIS portal where Luna should appear.")]
    public Transform spawnPoint; // drag your existing child here

    [Tooltip("Name of the child under ScaledLuna that should align to the spawn point.")]
    public string playerChildName = "Luna";

    void Start()
    {
        if (PortalState.lastUsedPortal != portalID) return;
        if (!spawnPoint)
        {
            // fallback: try to find a child named "SpawnPoint" under this portal
            var fallback = transform.Find("SpawnPoint");
            if (fallback) spawnPoint = fallback;
            if (!spawnPoint) { Debug.LogWarning("PortalSpawn: No spawnPoint assigned."); return; }
        }

        var playerRoot = GameObject.FindGameObjectWithTag("Player"); // ScaledLuna root
        if (!playerRoot) return;

        // Find the child named "Luna" under ScaledLuna
        Transform lunaChild = playerRoot.transform.Find(playerChildName);
        if (!lunaChild)
        {
            // robust fallback: search all children by name
            foreach (var t in playerRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == playerChildName) { lunaChild = t; break; }
            }
            if (!lunaChild)
            {
                Debug.LogWarning($"PortalSpawn: Could not find child '{playerChildName}' under Player.");
                // As a last resort, just move the root to the spawn point
                playerRoot.transform.position = spawnPoint.position;
                return;
            }
        }

        // Compute world-space offset from root to the Luna child
        Vector3 rootToChildOffset = lunaChild.position - playerRoot.transform.position;

        // Place the root so that the child lands exactly on spawnPoint
        playerRoot.transform.position = spawnPoint.position - rootToChildOffset;

        // If Luna's Rigidbody2D is on the child, zero velocity to avoid drift
        var rb = lunaChild.GetComponent<Rigidbody2D>();
        if (!rb) rb = playerRoot.GetComponentInChildren<Rigidbody2D>();
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // Ensure the physics body is exactly at the spawn point
            rb.position = spawnPoint.position;
        }

        // Optional: clear after use so it doesn't retrigger
        // PortalState.lastUsedPortal = "";
    }
}
