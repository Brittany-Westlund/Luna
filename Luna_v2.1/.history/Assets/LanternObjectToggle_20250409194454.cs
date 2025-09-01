using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    [Header("Lantern Settings")]
    public SpriteRenderer LitLanternSprite;
    public MMProgressBar lightBar;
    public KeyCode activationKey = KeyCode.E;
    public float lightCost = 0.1f;

    [Header("Linked Object Settings")]
    public GameObject targetObject;                 // e.g., platform to activate/deactivate
    public bool deactivateOnLight = true;
    public GameObject linkedObject;                 // e.g., mushroom to show/hide
    public SpriteRenderer additionalSpriteToToggle; // 👈 new optional sprite to hide/show

    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange || lightBar == null || LitLanternSprite == null) return;

        if (Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;

            if (!LitLanternSprite.enabled && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                LitLanternSprite.enabled = true;
                ToggleTargetObject(!deactivateOnLight);
                HandleLinkedObject(true);
                ToggleExtraSprite(true);
            }
            else if (LitLanternSprite.enabled)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                LitLanternSprite.enabled = false;
                ToggleTargetObject(deactivateOnLight);
                HandleLinkedObject(false);
                ToggleExtraSprite(false);
            }
        }
        else
        {
            // Continuously reflect lantern sprite state to linked visuals
            HandleLinkedObject(LitLanternSprite.enabled);
            ToggleExtraSprite(LitLanternSprite.enabled);
        }
    }

    private void ToggleTargetObject(bool state)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(state);
        }
    }

    private void HandleLinkedObject(bool enable)
    {
        if (linkedObject == null) return;

        linkedObject.SetActive(enable);

        SpriteRenderer sr = linkedObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = enable;
        }
    }

    private void ToggleExtraSprite(bool enable)
    {
        if (additionalSpriteToToggle != null)
        {
            additionalSpriteToToggle.enabled = enable;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
