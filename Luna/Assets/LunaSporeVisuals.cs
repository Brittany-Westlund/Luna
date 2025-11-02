using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LunaAnimatorToggle))]
public class LunaGlowCompanion : MonoBehaviour
{
    [Header("Spore Visuals")]
    [Tooltip("Color applied to Luna's spores in glow mode.")]
    public Color glowColor = Color.white;

    [Header("Flower Auto-Light")]
    public float lightRadius = 1.5f;
    public LayerMask sproutLayer;

    [Header("Debug")]
    public bool logActions = false;

    private LunaAnimatorToggle animatorToggle;
    private Transform attachPoint;
    private bool glowModeActive = false;
    private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    void Awake()
    {
        animatorToggle = GetComponent<LunaAnimatorToggle>();
        if (animatorToggle == null)
        {
            Debug.LogError("❌ LunaAnimatorToggle not found on Luna.");
            enabled = false;
            return;
        }

        attachPoint = transform.Find("AttachPoint") ?? transform;
    }

    void Update()
    {
        bool isGlowing = animatorToggle.IsGlowAnimatorActive();

        // If mode changed, update all spores immediately
        if (isGlowing != glowModeActive)
        {
            glowModeActive = isGlowing;
            if (glowModeActive)
                ApplyGlowColor();
            else
                RestoreOriginalColors();

            if (logActions)
                Debug.Log($"✨ Glow mode toggled: {glowModeActive}");
        }

        // Continuously handle new spores
        if (glowModeActive)
            MaintainGlowOnNewSpores();

        // Auto-light flowers while glowing
        if (glowModeActive)
            AutoLightNearbyFlowers();
    }

    private void ApplyGlowColor()
    {
        foreach (var sr in GetAttachedSpores())
        {
            if (!originalColors.ContainsKey(sr))
                originalColors[sr] = sr.color;

            sr.color = glowColor;
        }
    }

    private void MaintainGlowOnNewSpores()
    {
        foreach (var sr in GetAttachedSpores())
        {
            // If a new spore appears during glow mode
            if (!originalColors.ContainsKey(sr))
                originalColors[sr] = sr.color;

            if (sr.color != glowColor)
                sr.color = glowColor;
        }
    }

    private void RestoreOriginalColors()
    {
        foreach (var sr in new List<SpriteRenderer>(originalColors.Keys))
        {
            if (sr == null)
            {
                originalColors.Remove(sr);
                continue;
            }

            sr.color = originalColors[sr];
            originalColors.Remove(sr);
        }
    }

    private IEnumerable<SpriteRenderer> GetAttachedSpores()
    {
        if (attachPoint == null) yield break;

        foreach (Transform child in attachPoint)
        {
            if (child == null) continue;
            SpriteRenderer sr = child.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                yield return sr;
        }
    }

    private void AutoLightNearbyFlowers()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, lightRadius, sproutLayer);
        foreach (var col in nearby)
        {
            var sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.IsFullyGrown)
            {
                Transform litChild = sprout.transform.Find("LitFlowerB");
                if (litChild != null)
                {
                    var sr = litChild.GetComponent<SpriteRenderer>();
                    if (sr != null && !sr.enabled)
                    {
                        sr.enabled = true;
                        if (logActions)
                            Debug.Log($"💡 Auto-lit {sprout.name}");
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.8f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }
}
