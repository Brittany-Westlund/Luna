using UnityEngine;
using MoreMountains.Tools;

public class LanternSpriteToggle : MonoBehaviour
{
    [Header("Lantern Visuals")]
    public SpriteRenderer litLanternSprite;          // The lit lantern's visual

    [Header("Light Bar Settings")]
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;

    [Header("Targets to Toggle")]
    public SpriteRenderer[] spriteTargetsToToggle;   // Any environment visuals like mushrooms
    public SpriteRenderer extraSprite;               // Optional extra sprite to toggle

    [Header("Behavior")]
    private bool playerInRange = false;
    private bool isLit = false;
    private KeyCode activationKey = KeyCode.E;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar != null ? lightBar.BarProgress : 0f;

            if (!isLit && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                SetLitState(true);
            }
            else if (isLit)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                SetLitState(false);
            }
        }
    }

    private void SetLitState(bool turnOn)
    {
        isLit = turnOn;

        if (litLanternSprite != null)
            litLanternSprite.enabled = turnOn;

        foreach (var sprite in spriteTargetsToToggle)
        {
            if (sprite != null)
                sprite.enabled = turnOn;
        }

        if (extraSprite != null)
            extraSprite.enabled = turnOn;

        Debug.Log(turnOn ? "🔆 Lantern lit and sprites shown." : "🌑 Lantern turned off.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
