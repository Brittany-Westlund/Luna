using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SparkleColorToggle : MonoBehaviour
{
    [Header("Collisions")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool logDebug = true;

    private SpriteRenderer[] childRenderers;
    private Color[] originalColors;

    void Awake()
    {
        childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            originalColors[i] = childRenderers[i].color;
            if (logDebug) Debug.Log($"[{name}] Child {childRenderers[i].name} original color = {originalColors[i]}");
        }

        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (logDebug) Debug.Log($"[{name}] Trigger ENTER by {other.name}");

        for (int i = 0; i < childRenderers.Length; i++)
        {
            var r = childRenderers[i];
            if (r == null) continue;

            Color c = r.color;
            r.color = new Color(1f, 1f, 1f, c.a);

            if (logDebug) Debug.Log($"[{name}] Set {r.name} → {r.color}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (logDebug) Debug.Log($"[{name}] Trigger EXIT by {other.name}");

        for (int i = 0; i < childRenderers.Length; i++)
        {
            var r = childRenderers[i];
            if (r == null) continue;

            Color now = r.color;
            Color orig = originalColors[i];
            r.color = new Color(orig.r, orig.g, orig.b, now.a);

            if (logDebug) Debug.Log($"[{name}] Restored {r.name} → {r.color}");
        }
    }
}
