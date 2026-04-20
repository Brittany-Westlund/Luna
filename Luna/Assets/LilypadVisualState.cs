using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LilypadVisualState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Trigger")]
    [SerializeField] private string[] validTriggerTags = new string[] { "PlayerFeet", "Player" };

    [Header("Particle References (optional)")]
    [Tooltip("If assigned, only these Pulsate components will be affected.")]
    [SerializeField] private List<Pulsate> assignedPulsates = new List<Pulsate>();

    [Header("Particle Tag Fallback")]
    [SerializeField] private string particleTag = "LotusParticle";
    [SerializeField] private bool useTagFallback = false;

    [Header("Pad Color")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private bool preserveOriginalAlpha = true;

    [Header("Particle Response")]
    [SerializeField] private float activeScaleMultiplier = 1.35f;
    [SerializeField] private float activeSpeedMultiplier = 1.3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<Pulsate> allPulsates = new List<Pulsate>();
    private readonly HashSet<Collider2D> validOverlaps = new HashSet<Collider2D>();

    private Collider2D triggerCollider;
    private Color originalColor;
    private bool isActive = false;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[LilypadVisualState] No SpriteRenderer assigned/found on {name}.");
            return;
        }

        originalColor = spriteRenderer.color;

        RefreshPulsateCache();
        ResetAllPulsatesToDefault();
        ApplyInactiveVisualsImmediate();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] Awake on {name}");
            Debug.Log($"[LilypadVisualState] Renderer = {spriteRenderer.name}");
            Debug.Log($"[LilypadVisualState] Original color = {originalColor}");
            Debug.Log($"[LilypadVisualState] Pulsates found = {allPulsates.Count}");
        }
    }

    private void OnEnable()
    {
        validOverlaps.Clear();
        isActive = false;

        RefreshPulsateCache();
        ResetAllPulsatesToDefault();
        ApplyInactiveVisualsImmediate();
    }

    private void OnDisable()
    {
        validOverlaps.Clear();
        isActive = false;

        ResetAllPulsatesToDefault();
        ApplyInactiveVisualsImmediate();
    }

    private void OnValidate()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        // Force color every frame so no animator/other script can quietly override it.
        if (isActive)
            ApplyActiveVisualsImmediate();
        else
            ApplyInactiveVisualsImmediate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        validOverlaps.Add(other);

        if (debugLogs)
            Debug.Log($"[LilypadVisualState] ENTER {name} by {other.name}, tag={other.tag}, overlaps={validOverlaps.Count}");

        if (!isActive)
            ActivateVisuals();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        validOverlaps.Add(other);

        if (!isActive)
        {
            if (debugLogs)
                Debug.Log($"[LilypadVisualState] STAY re-activating {name} by {other.name}, tag={other.tag}");

            ActivateVisuals();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        validOverlaps.Remove(other);

        if (debugLogs)
            Debug.Log($"[LilypadVisualState] EXIT {name} by {other.name}, tag={other.tag}, overlaps={validOverlaps.Count}");

        if (validOverlaps.Count == 0)
            DeactivateVisuals();
    }

    private bool IsValidTrigger(Collider2D other)
    {
        if (other == null)
            return false;

        for (int i = 0; i < validTriggerTags.Length; i++)
        {
            string tagToCheck = validTriggerTags[i];
            if (!string.IsNullOrEmpty(tagToCheck) && other.CompareTag(tagToCheck))
                return true;
        }

        return false;
    }

    private void ActivateVisuals()
    {
        isActive = true;
        ApplyActiveVisualsImmediate();

        RefreshPulsateCache();

        for (int i = 0; i < allPulsates.Count; i++)
        {
            Pulsate p = allPulsates[i];
            if (p == null)
                continue;

            p.externalScaleMultiplier = activeScaleMultiplier;
            p.externalSpeedMultiplier = activeSpeedMultiplier;
        }

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] Activated {name}. Renderer={spriteRenderer.name}, Color={spriteRenderer.color}, Pulsates affected={allPulsates.Count}");
        }
    }

    private void DeactivateVisuals()
    {
        isActive = false;

        ApplyInactiveVisualsImmediate();
        ResetAllPulsatesToDefault();

        if (debugLogs)
            Debug.Log($"[LilypadVisualState] Deactivated {name}. Renderer={spriteRenderer.name}, Color={spriteRenderer.color}");
    }

    private void ApplyActiveVisualsImmediate()
    {
        if (spriteRenderer == null)
            return;

        Color c = activeColor;
        if (preserveOriginalAlpha)
            c.a = originalColor.a;

        spriteRenderer.color = c;
    }

    private void ApplyInactiveVisualsImmediate()
    {
        if (spriteRenderer == null)
            return;

        Color c = inactiveColor;
        if (preserveOriginalAlpha)
            c.a = originalColor.a;

        spriteRenderer.color = c;
    }

    private void RefreshPulsateCache()
    {
        allPulsates.Clear();

        for (int i = 0; i < assignedPulsates.Count; i++)
        {
            Pulsate p = assignedPulsates[i];
            if (p != null && !allPulsates.Contains(p))
                allPulsates.Add(p);
        }

        if (allPulsates.Count > 0)
            return;

        if (!useTagFallback)
            return;

        GameObject[] objs;
        try
        {
            objs = GameObject.FindGameObjectsWithTag(particleTag);
        }
        catch
        {
            if (debugLogs)
                Debug.LogWarning($"[LilypadVisualState] Tag '{particleTag}' does not exist.");

            return;
        }

        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] == null)
                continue;

            Pulsate p = objs[i].GetComponent<Pulsate>();
            if (p != null && !allPulsates.Contains(p))
                allPulsates.Add(p);
        }
    }

    private void ResetAllPulsatesToDefault()
    {
        for (int i = 0; i < allPulsates.Count; i++)
        {
            Pulsate p = allPulsates[i];
            if (p == null)
                continue;

            p.externalScaleMultiplier = 1f;
            p.externalSpeedMultiplier = 1f;
        }
    }
}