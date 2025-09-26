// PortalSpawn.cs (destination scene)
using UnityEngine;
using System.Collections;

public class PortalSpawn : MonoBehaviour
{
    public string portalID;          // must exactly match LevelPortal.portalID
    public Transform spawnPoint;     // optional; if null uses this.transform
    public string playerChildName = "Luna"; // optional

    void OnEnable() { Debug.Log($"[PORTAL SPAWN:{portalID}] OnEnable. lastUsedPortal='{PortalState.lastUsedPortal}'");
    StartCoroutine(Place()); }

    IEnumerator Place()
    {
        if (PortalState.lastUsedPortal != portalID) yield break;

        if (!spawnPoint) spawnPoint = transform;
        yield return null; // wait so Player exists
        Debug.Log($"[PORTAL SPAWN:{portalID}] After frame. lastUsedPortal='{PortalState.lastUsedPortal}'");
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) yield break;

        Transform child = null;
        if (!string.IsNullOrEmpty(playerChildName))
        {
            child = player.transform.Find(playerChildName);
            if (!child)
                foreach (var t in player.GetComponentsInChildren<Transform>(true))
                    if (t.name == playerChildName) { child = t; break; }
        }

        Vector3 off = child ? (child.position - player.transform.position) : Vector3.zero;
        player.transform.position = spawnPoint.position - off;

        var rb = (child ? child.GetComponent<Rigidbody2D>() : null)
                 ?? player.GetComponentInChildren<Rigidbody2D>();
        if (rb) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; rb.position = spawnPoint.position; }

        Debug.Log($"[PORTAL SPAWN] matched id='{portalID}' at {spawnPoint.position}");
        Debug.Log($"[PORTAL SPAWN:{portalID}] Placed at {spawnPoint.position}");
    
        PortalState.lastUsedPortal = ""; // ✅ consume it so it doesn't retrigger later
    }
}
