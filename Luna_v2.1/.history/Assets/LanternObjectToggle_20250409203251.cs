using UnityEngine;
using MoreMountains.Tools;

public class LanternSpriteToggleExact : MonoBehaviour
{
    [Header("Lantern Visual")]
    public SpriteRenderer litLanternSprite;

    [Header("Light System")]
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;
    private bool isLit = false;

    [Header("Sprite Toggles")]
    public SpriteRenderer spriteToEnable;   // This turns ON when lantern is lit
    public SpriteRenderer spriteToDisable;  // This turns OFF when lantern is lit

    [Header("Input")]
    public KeyCode activationKey = KeyCode.E;

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(activationKey))
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

        if (spriteToEnable != null)
            spriteToEnable.enabled = turnOn;

        if (spriteToDisable != null)
            spriteToDisable.enabled = !turnOn;

        Debug.Log(turnOn ? "✅ Lantern turned ON" : "❌ Lantern turned OFF");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
