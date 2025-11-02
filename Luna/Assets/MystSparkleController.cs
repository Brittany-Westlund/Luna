using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LightActivatesMistSparkles_Proximity : MonoBehaviour
{
    [Header("Detection")]
    public float activationRadius = 2.5f;
    public bool requireThisRendererLit = true;
    public bool checkChildrenForRenderer = true;
    public SpriteRenderer sourceRenderer;

    [Header("Light Source Settings")]
    public bool includeMoonflowers = true;
    public bool includeLightMotes = true;
    public bool includeWands = true;

    [Header("Debug")]
    public bool debugLogs = false;

    private readonly HashSet<Transform> _alreadyActivated = new HashSet<Transform>();

    void Start()
    {
        if (sourceRenderer == null)
        {
            if (checkChildrenForRenderer)
                sourceRenderer = GetComponentInChildren<SpriteRenderer>(true);
            else
                sourceRenderer = GetComponent<SpriteRenderer>();
        }
    }

    // 🕓 Run after all Updates → ensures objects are active before enabling sparkles
    void LateUpdate()
    {
        if (!IsValidLightSource(gameObject))
            return;

        var hits = Physics2D.OverlapCircleAll(transform.position, activationRadius);
        if (hits == null || hits.Length == 0) return;

        foreach (var hit in hits)
        {
            if (hit == null || !hit.CompareTag("Mist")) continue;
            var mist = hit.transform;

            if (_alreadyActivated.Contains(mist)) continue;

            var sparkles = mist.Find("Sparkles");
            if (sparkles != null)
            {
                StartCoroutine(ActivateNextFrame(sparkles.gameObject, mist));
            }
            else if (debugLogs)
            {
                Debug.LogWarning($"⚠️ Mist '{mist.name}' has no child named 'Sparkles'");
            }
        }
    }

    private IEnumerator ActivateNextFrame(GameObject sparkles, Transform mist)
    {
        yield return null; // wait one frame for physics + hierarchy sync
        sparkles.SetActive(true);
        _alreadyActivated.Add(mist);
        if (debugLogs)
            Debug.Log($"✨ [{name}] activated sparkles on Mist '{mist.name}'");
    }

    private bool IsValidLightSource(GameObject obj)
    {
        // 🌕 LUNA — only glow form counts
        if (obj.CompareTag("Player"))
        {
            var glow = obj.GetComponent<LunaGlowToggle>();
            if (glow == null) return false;
            return glow.IsGlowing;
        }

        // 🌼 Moonflower
        if (includeMoonflowers && obj.name.Contains("Moonflower"))
        {
            var litNode = FindDeepChild(obj.transform, "LitFlowerB");
            if (litNode != null)
            {
                var anyLit = litNode.GetComponentsInChildren<SpriteRenderer>(true)
                    .Any(r => r.enabled && r.gameObject.activeInHierarchy);
                if (anyLit) return true;
            }
        }

        // 🔮 LightMote
        if (includeLightMotes && obj.CompareTag("LightMote"))
            return true;

        // 🪄 Wand
        if (includeWands && obj.name.Contains("LitWand"))
            return true;

        // Fallback
        if (!requireThisRendererLit) return true;
        if (sourceRenderer != null && sourceRenderer.enabled && sourceRenderer.gameObject.activeInHierarchy)
            return true;

        return false;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        var t = parent.Find(childName);
        if (t != null) return t;

        for (int i = 0; i < parent.childCount; i++)
        {
            var result = FindDeepChild(parent.GetChild(i), childName);
            if (result != null) return result;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.5f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
