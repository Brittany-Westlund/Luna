using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SparkleFadeWithLunaria : MonoBehaviour
{
    [Header("References")]
    public LunariaGlowFromLightSources_Array lunaria;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    private List<SpriteRenderer> sparkleSprites = new List<SpriteRenderer>();
    private List<Pulsate> pulsateScripts = new List<Pulsate>();
    public float targetAlpha = 0f;

    private void Start()
    {
        // Find all sparkle renderers & any Pulsate scripts under this object
        sparkleSprites.AddRange(GetComponentsInChildren<SpriteRenderer>());
        pulsateScripts.AddRange(GetComponentsInChildren<Pulsate>());

        if (lunaria == null)
            lunaria = GetComponentInParent<LunariaGlowFromLightSources_Array>();

        if (sparkleSprites.Count == 0)
        {
            Debug.LogWarning($"{name}: No sparkle SpriteRenderers found!");
            enabled = false;
            return;
        }

        // Start hidden
        foreach (var sr in sparkleSprites)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        // Disable all Pulsate scripts at start
        foreach (var p in pulsateScripts)
            p.enabled = false;

        StartCoroutine(WatchGlow());
    }

    private IEnumerator WatchGlow()
    {
        while (true)
        {
            bool shouldGlow = lunaria != null && lunaria.IsGlowing; // public getter from LunariaGlowFromFlowerType
            targetAlpha = shouldGlow ? maxAlpha : 0f;

            // Enable/disable pulsing
            foreach (var p in pulsateScripts)
                p.enabled = shouldGlow;

            // Fade all sparkles
            foreach (var sr in sparkleSprites)
            {
                Color c = sr.color;
                c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
                sr.color = c;
            }

            yield return null;
        }
    }
}
