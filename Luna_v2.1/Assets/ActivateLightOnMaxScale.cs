using UnityEngine;

public class ActivateLightOnMaxScale : MonoBehaviour
{
    public float maxScale = 0.22f; // Set the maximum scale at which to activate the light
    public MonoBehaviour enableLitFlowerScript; // Reference to the script that controls the LitFlower

    void Start()
    {
        if (enableLitFlowerScript == null)
        {
            Debug.LogError("The control script for LitFlower is not assigned.");
        }
        else
        {
            // Ensure the control script for LitFlower starts off disabled
            enableLitFlowerScript.enabled = false;
        }
    }

    void Update()
    {
        // Check if the sprout has reached its maximum scale
        if (transform.localScale.x >= maxScale)
        {
            // If it has, and the LitFlower control script is not yet enabled, enable it
            if (enableLitFlowerScript != null && !enableLitFlowerScript.enabled)
            {
                enableLitFlowerScript.enabled = true; // Enable the control script for LitFlower
            }
        }
    }
}
