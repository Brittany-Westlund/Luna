using UnityEngine;
using MoreMountains.Tools;

public class LanternSmartToggle : MonoBehaviour
{
    [Header("Lantern Light Source")]
    public SpriteRenderer litLanternSprite;

    [Header("Optional: Control by Key")]
    public bool allowPlayerToggle = false;
    public KeyCode toggleKey = KeyCode.E;
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;

    [Header("What to Enable When Lit")]
    public SpriteRenderer[] spritesToEnable;
    public GameObject[] objectsToEnable;

    [Header("What to Disable When Lit")]
    public SpriteRenderer[] spritesToDisable;
    public GameObject[] objectsToDisable;

    private bool lastLitState = false;
    private bool playerInRange = false;

    void Start()
    {
        if (litLanternSprite != null)
        {
            lastLitState = litLanternSprite.enabled;
            ApplyToggleState(lastLitState);
        }
    }

    void Update()
    {
        if (allowPlayerToggle && playerInRange && Input.GetKeyDown(toggleKey))
        {
            ToggleLanternManually();
        }

        // Always apply the correct state when the sprite renderer changes
        if (litLanternSprite != null && litLanternSprite.enabled != lastLitState)
        {
            ApplyToggleState(litLanternSprite.enabled);
            lastLitState = litLanternSprite.enabled;
        }
    }

    private void ToggleLanternManually()
    {
        if (litLanternSprite == null || lightBar == null) return;

        float current = lightBar.BarProgress;

        if (!litLanternSprite.enabled && current >= lightCost)
        {
            litLanternSprite.enabled = true;
            lightBar.SetBar01(current - lightCost);
        }
        else if (litLanternSprite.enabled)
        {
            litLanternSprite.enabled = false;
            lightBar.SetBar01(current + lightCost);
        }

        // The Update() loop will catch the change and apply visuals
    }

    private void ApplyToggleState(bool isLit)
    {
        foreach (var sr in spritesToEnable)
            if (sr != null) sr.enabled = isLit;

        foreach (var go in objectsToEnable)
            if (go != null) go.SetActive(isLit);

        foreach (var sr in spritesToDisable)
            if (sr != null) sr.enabled = !isLit;

        foreach (var go in objectsToDisable)
            if (go != null) go.SetActive(!isLit);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (allowPlayerToggle && other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (allowPlayerToggle && other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
