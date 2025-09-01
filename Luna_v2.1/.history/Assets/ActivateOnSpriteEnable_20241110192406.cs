using UnityEngine;

public class ActivateOnSpriteEnable : MonoBehaviour
{
    public GameObject targetObject; // The GameObject to enable when the SpriteRenderer is enabled

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the target object is initially disabled
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if the SpriteRenderer is enabled
        if (spriteRenderer != null && spriteRenderer.enabled)
        {
            // Enable the target object
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }

            // Optionally, destroy this script if you only want it to run once
            Destroy(this);
        }
    }
}
