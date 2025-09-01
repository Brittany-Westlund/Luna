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
        // Optional: allow player toggle
        if (allowPlayerToggle && playerInRange && Input.GetKeyDown(toggleKey))
        {
            if (lightBar != null && lightBar.BarProgress >= lightCost)
            {
                isLit = !isLit;
                litLanternSprite.enabled = isLit;
                lightBar.SetBar01(lightBar.BarProgress + (isLit ? -lightCost : lightCost));
                ApplyToggleState(isLit);
            }
            else if (!isLit)
            {
                Debug.Log("⚠️ Not enough light to activate the lantern.");
            }
        }

        // Passive syncing from other scripts
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
