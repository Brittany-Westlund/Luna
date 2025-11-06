using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 🩹 Ensures the LightMote layer, tag, and physics collision matrix are valid at runtime.
/// Attach this to a persistent GameObject (like GameManager, DialogueManager, etc.).
/// </summary>
[DefaultExecutionOrder(-10000)] // runs before everything else
public class LightMotePhysicsFixer : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Exact layer name used by your light motes.")]
    public string moteLayerName = "LightMote";

    [Tooltip("Exact tag used by your light motes.")]
    public string moteTagName = "LightMote";

    [Tooltip("Tag assigned to the wand GameObject.")]
    public string wandTag = "Wand";

    [Header("Diagnostics")]
    public bool logFixes = true;

    void Awake()
    {
        int moteLayer = LayerMask.NameToLayer(moteLayerName);
        if (moteLayer == -1)
        {
            Debug.LogWarning($"[LightMoteFixer] Layer '{moteLayerName}' not found. Using default collision mask.");
            return;
        }

        // find the wand object
        var wand = GameObject.FindWithTag(wandTag);
        if (wand == null)
        {
            Debug.LogWarning($"[LightMoteFixer] No GameObject found with tag '{wandTag}'.");
            return;
        }

        int wandLayer = wand.layer;

        // ✅ Ensure physics collisions between wand and motes are enabled
        Physics2D.IgnoreLayerCollision(wandLayer, moteLayer, false);
        if (logFixes)
            Debug.Log($"[LightMoteFixer] Enabled collisions between Wand layer ({wandLayer}) and LightMote layer ({moteLayer}).");

        // ✅ Fix existing LightMote objects in the scene
        var motes = GameObject.FindGameObjectsWithTag(moteTagName);
        foreach (var m in motes)
        {
            if (m.layer != moteLayer)
            {
                m.layer = moteLayer;
                if (logFixes)
                    Debug.Log($"[LightMoteFixer] Fixed layer on mote '{m.name}'.");
            }

            // ensure physics setup
            var rb = m.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = m.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
            }

            var col = m.GetComponent<Collider2D>();
            if (col == null)
            {
                col = m.AddComponent<CircleCollider2D>();
            }

            col.enabled = true;
        }

        if (logFixes)
            Debug.Log($"[LightMoteFixer] Validated {motes.Length} LightMote objects in '{SceneManager.GetActiveScene().name}'.");
    }
}
