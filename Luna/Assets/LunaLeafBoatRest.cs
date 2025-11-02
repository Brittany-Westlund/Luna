using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LunaLeafBoatRest : MonoBehaviour
{
    private LunaRest lunaRest;
    private Transform lunaTransform;
    private Collider2D leafBoatCollider;
    private bool lunaIsRestingHere = false;

    void Awake()
    {
        leafBoatCollider = GetComponent<Collider2D>();
        leafBoatCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        lunaRest = col.GetComponent<LunaRest>();
        lunaTransform = col.transform;

        if (lunaRest == null)
        {
            Debug.LogWarning("🚫 LunaRest component not found on Player!");
            return;
        }

        lunaRest.BeginRestExternal();
        lunaIsRestingHere = true;

        Debug.Log("🍃 Luna enters LeafBoat and begins resting.");
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (!lunaIsRestingHere || lunaRest == null) return;

        // Detect if rest somehow stopped externally
        if (!lunaRest.isResting)
        {
            Debug.Log("⚠️ LunaRest switched to NOT resting during LeafBoat stay. Something else ended rest!");
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        if (lunaRest != null)
        {
            lunaRest.EndRestExternal();
            Debug.Log("💨 Luna leaves the LeafBoat trigger — rest ended.");
        }

        // Disable collider so this leafboat can’t trigger again
        if (leafBoatCollider != null)
        {
            leafBoatCollider.enabled = false;
            Debug.Log("🪶 LeafBoat collider disabled after Luna exited.");
        }

        lunaIsRestingHere = false;
        lunaRest = null;
        lunaTransform = null;
    }

    void OnDisable()
    {
        if (lunaIsRestingHere)
        {
            Debug.Log("🧹 LeafBoat disabled while Luna was resting — force-ending rest.");
        }
    }
}
