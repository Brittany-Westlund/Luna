using UnityEngine;

public class ActivateOnSpriteEnable : MonoBehaviour
{
    public GameObject targetObject; // The GameObject to enable/disable based on SpriteRenderer
    public bool toggleWithSprite = false; // Option to toggle the target object on/off with the SpriteRenderer

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the target object is initially set based on the SpriteRenderer state
        if (targetObject != null)
        {
            targetObject.SetActive(spriteRenderer.enabled);
        }
    }

    private void Update()
    {
        // Check if the SpriteRenderer is enabled or disabled
        if (spriteRenderer != null && targetObject != null)
        {
            // Toggle the target object based on the SpriteRenderer's state
            if (toggleWithSprite)
            {
                targetObject.SetActive(spriteRenderer.enabled);
            }
            else
            {
                // Only activate once when the SpriteRenderer is enabled
                if (spriteRenderer.enabled && !targetObject.activeSelf)
                {
                    targetObject.SetActive(true);
                    Destroy(this); // Optionally destroy the script if it only needs to activate once
                }
            }
        }
    }
}
