using UnityEngine;

public class DebugSpriteRendererLifecycle : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Log($"[{name}] Awake — SpriteRenderer ref is {sr}");
    }

    void OnEnable()
    {
        Debug.Log($"[{name}] ENABLED");
    }

    void OnDisable()
    {
        Debug.Log($"[{name}] DISABLED");
    }

    void OnDestroy()
    {
        Debug.Log($"[{name}] DESTROYED");
    }

    void Update()
    {
        if (sr == null || sr.Equals(null))
        {
            Debug.LogWarning($"[{name}] SpriteRenderer ref went MISSING!");
        }
    }
}
