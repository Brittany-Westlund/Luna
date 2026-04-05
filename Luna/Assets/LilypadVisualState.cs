using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LilypadVisualState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Trigger")]
    [SerializeField] private string triggerTag = "PlayerFeet";

    [Header("Particle Tag")]
    [SerializeField] private string particleTag = "LotusParticle";

    [Header("Pad Color")]
    [SerializeField] private Color activeColor = Color.white;

    [Header("Particle Response")]
    [SerializeField] private float activeScaleMultiplier = 1.35f;
    [SerializeField] private float activeSpeedMultiplier = 1.3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private readonly List<Pulsate> allPulsates = new List<Pulsate>();

    private Collider2D triggerCollider;
    private Color originalColor;
    private bool isActive = false;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[LilypadVisualState] No SpriteRenderer found on {name}.");
            return;
        }

        originalColor = spriteRenderer.color;

        RefreshPulsateCache();
        ResetAllPulsatesToDefault();
        ApplyInactiveVisualsImmediate();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] Awake on {name}");
            Debug.Log($"[LilypadVisualState] Original color = {originalColor}");
            Debug.Log($"[LilypadVisualState] Pulsates found = {allPulsates.Count}");
        }
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] ENTER {name} by {other.name}, tag={other.tag}");
        }

        ActivateVisuals();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        if (!isActive)
        {
            if (debugLogs)
            {
                Debug.Log($"[LilypadVisualState] STAY re-activating {name} by {other.name}, tag={other.tag}");
            }

            ActivateVisuals();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidTrigger(other))
            return;

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] EXIT {name} by {other.name}, tag={other.tag}");
        }

        DeactivateVisuals();
    }

    private bool IsValidTrigger(Collider2D other)
    {
        if (other == null)
            return false;

        return other.CompareTag(triggerTag);
    }

    private void ActivateVisuals()
    {
        isActive = true;

        if (spriteRenderer != null)
        {
            Color newColor = new Color(activeColor.r, activeColor.g, activeColor.b, originalColor.a);
            spriteRenderer.color = newColor;

            if (debugLogs)
            {
                Debug.Log($"[LilypadVisualState] Set ACTIVE color on {name} to {spriteRenderer.color}");
            }
        }

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
            Debug.Log($"[LilypadVisualState] Activated {name}. Pulsates affected: {allPulsates.Count}");
        }
    }

    private void DeactivateVisuals()
    {
        isActive = false;

        ApplyInactiveVisualsImmediate();

        RefreshPulsateCache();
        ResetAllPulsatesToDefault();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] Deactivated {name}. Color reset to {spriteRenderer.color}");
        }
    }

    private void ApplyInactiveVisualsImmediate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void RefreshPulsateCache()
    {
        allPulsates.Clear();

        GameObject[] objs;
        try
        {
            objs = GameObject.FindGameObjectsWithTag(particleTag);
        }
        catch
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[LilypadVisualState] Tag '{particleTag}' does not exist.");
            }

            return;
        }

        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] == null)
                continue;

            Pulsate p = objs[i].GetComponent<Pulsate>();
            if (p != null)
            {
                allPulsates.Add(p);
            }
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