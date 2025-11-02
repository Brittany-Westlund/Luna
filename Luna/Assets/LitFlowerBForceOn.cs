using UnityEngine;

/// <summary>
/// Keeps the "LitFlowerB" child sprite renderer always enabled,
/// overriding anything else that tries to disable it.
/// </summary>
public class LitFlowerBForceOn : MonoBehaviour
{
    [Tooltip("Optional: manually assign LitFlowerB if not found automatically.")]
    public SpriteRenderer litRenderer;

    [Tooltip("Keep checking every frame (set true if something else disables it later).")]
    public bool persistent = true;

    void Awake()
    {
        if (litRenderer == null)
            litRenderer = FindLitRenderer(transform);
    }

    void Start()
    {
        ForceEnable();
    }

    void Update()
    {
        if (persistent && litRenderer != null && !litRenderer.enabled)
            litRenderer.enabled = true;
    }

    void ForceEnable()
    {
        if (litRenderer != null)
        {
            litRenderer.enabled = true;
            litRenderer.gameObject.SetActive(true);
            Debug.Log($"🌕 Force-enabled LitFlowerB renderer on {name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No LitFlowerB found under {name}!");
        }
    }

    private SpriteRenderer FindLitRenderer(Transform parent)
    {
        // Recursively look for "LitFlowerB"
        foreach (Transform child in parent)
        {
            if (child.name == "LitFlowerB")
            {
                var r = child.GetComponent<SpriteRenderer>();
                if (r != null) return r;
            }
            var result = FindLitRenderer(child);
            if (result != null) return result;
        }
        return null;
    }
}
