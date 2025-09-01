using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    [Header("Lantern Visuals")]
    public SpriteRenderer LitLanternSprite;
    private KeyCode activationKey = KeyCode.E;

    [Header("Light Mechanics")]
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;
    private bool isLit = false;

    [Header("Object Activation Settings")]
    public GameObject targetObject;
    public bool deactivateOnLight = true;
    public GameObject linkedObject;

    [Header("Optional Sprite Control")]
    public SpriteRenderer additionalSpriteToToggle;         // Option 1: Direct SpriteRenderer
    public GameObject additionalSpriteObject;               // Option 2: GameObject (we'll fetch the SpriteRenderer from this)

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;

            if (!isLit && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                if (LitLanternSprite != null) LitLanternSprite.enabled = true;

                ToggleTargetObject(!deactivateOnLight);
                ToggleLinkedObject(true);
                ToggleExtraSprite(true);

                isLit = true;
            }
            else if (isLit)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                if (LitLanternSprite != null) LitLanternSprite.enabled = false;

                ToggleTargetObject(deactivateOnLight);
                ToggleLinkedObject(false);
                ToggleExtraSprite(false);

                isLit = false;
            }
        }
    }

    private void ToggleTargetObject(bool state)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(state);
        }
    }

    private void ToggleLinkedObject(bool enable)
    {
        if (linkedObject != null)
        {
            linkedObject.SetActive(enable);
        }
    }

    private void ToggleExtraSprite(bool enable)
    {
        if (additionalSpriteToToggle != null)
        {
            additionalSpriteToToggle.enabled = enable;
            return;
        }

        if (additionalSpriteObject != null)
        {
            SpriteRenderer sr = additionalSpriteObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = enable;
            }
            else
            {
                Debug.LogWarning("⚠️ SpriteRenderer not found on additionalSpriteObject.");
            }
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
