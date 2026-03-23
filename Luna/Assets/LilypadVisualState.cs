using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LilypadVisualState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Feet Trigger")]
    [SerializeField] private string feetTag = "PlayerFeet";

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

    private Color originalColor;
    private bool isActive = false;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        CacheTaggedPulsates();
        ResetAllPulsatesToDefault();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] Found {allPulsates.Count} LotusParticle pulsates.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(feetTag))
            return;

        ActivateVisuals();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] FEET ENTER {name}");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(feetTag))
            return;

        if (!isActive)
        {
            ActivateVisuals();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(feetTag))
            return;

        DeactivateVisuals();

        if (debugLogs)
        {
            Debug.Log($"[LilypadVisualState] FEET EXIT {name}");
        }
    }

    private void ActivateVisuals()
    {
        isActive = true;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.r = activeColor.r;
            c.g = activeColor.g;
            c.b = activeColor.b;
            c.a = originalColor.a;
            spriteRenderer.color = c;
        }

        for (int i = 0; i < allPulsates.Count; i++)
        {
            Pulsate p = allPulsates[i];
            if (p == null)
                continue;

            p.externalScaleMultiplier = activeScaleMultiplier;
            p.externalSpeedMultiplier = activeSpeedMultiplier;
        }
    }

    private void DeactivateVisuals()
    {
        isActive = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        ResetAllPulsatesToDefault();
    }

    private void CacheTaggedPulsates()
    {
        allPulsates.Clear();

        GameObject[] objs = GameObject.FindGameObjectsWithTag(particleTag);

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