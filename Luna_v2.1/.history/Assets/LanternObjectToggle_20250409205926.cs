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
    private bool isLit = false;
    private bool playerInRange = false;

    void Start()
    {
        if (litLanternSprite != null)
        {
            isLit = litLanternSprite.enabled;
            ApplyToggleState(isLit);
        }
    }

    void Update()
    {
        if (allowPlayerToggle && playerInRange && Input.GetKeyDown(toggleKey))
        {
            ToggleLanternManually();
        }

        if (!allowPlayerToggle && litLanternSprite != null)
        {
            if (litLanternSprite.enabled != lastLitState)
            {
                isLit = litLanternSprite.enabled;
                ApplyToggleState(isLit);
            }
        }

        lastLitState = isLit;
    }

    private void ToggleLanternManually()
    {
        if (litLanternSprite == null || lightBar == null) return;

        float currentProgress = lightBar.BarProgress;

        if (!isLit && currentProgress >= lightCost)
        {
            // Give light
            isLit = true;
            litLanternSprite.enabled = true;
            lightBar.SetBar01(currentProgress - lightCost);
            ApplyToggleState(true);
        }
        else if (isLit)
        {
            // Take light back
            isLit = false;
            litLanternSprite.enabled = false;
            lightBar.SetBar01(currentProgress + lightCost);
            ApplyToggleState(false);
        }
    }

    private void ApplyToggleState(bool lit)
    {
        foreach (var sr in spritesToEnable)
            if (sr != null) sr.enabled = lit;

        foreach (var go in objectsToEnable)
            if (go != null) go.SetActive(lit);

        foreach (var sr in spritesToDisable)
            if (sr != null) sr.enabled = !lit;

        foreach (var go in objectsToDisable)
            if (go != null) go.SetActive(!lit);
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
