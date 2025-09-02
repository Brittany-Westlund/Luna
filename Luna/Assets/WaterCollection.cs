using UnityEngine;

public class WaterCollection : MonoBehaviour
{
    public SpriteRenderer waterRenderer; // Reference to the Water sprite renderer

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            // Get the SpriteRenderer of the collided Water object
            SpriteRenderer otherWaterRenderer = other.GetComponent<SpriteRenderer>();
            if (otherWaterRenderer != null && otherWaterRenderer.enabled)
            {
                // Activate the waterRenderer of the ladle vessel if the collided Water's renderer is enabled
                waterRenderer.enabled = true;

                // Deactivate the waterRenderer of the collided Water object
                otherWaterRenderer.enabled = false;

                // Assuming you have a reference to the WellVineMovement script
                WellVineMovement wellVineScript = FindObjectOfType<WellVineMovement>();
                if (wellVineScript != null)
                {
                    wellVineScript.TurnOffWater();
                }
            }
        }
    }
}
