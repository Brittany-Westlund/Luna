using UnityEngine;

public class Watering : MonoBehaviour
{
    private SpriteRenderer sproutRenderer;
    public float growthIncrement = 0.1f;
    public float yPositionIncrement = 0.05f;
    public float maxScale = 0.2215f; // Maximum scale allowed for the sprout
    public float maxHeight = -1.098f; // Maximum height the sprout can move to
    public float lastWaterTime = -30f; // Track the last water time
    public float waterCooldown = 30f; // Cooldown in seconds

    void Start()
    {
        sproutRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            SpriteRenderer waterRenderer = other.GetComponent<SpriteRenderer>();
            if (waterRenderer != null && waterRenderer.enabled)
            {
                if (Time.time - lastWaterTime >= waterCooldown) // Check if enough time has passed
                {
                    if (!sproutRenderer.enabled)
                    {
                        sproutRenderer.enabled = true;
                    }

                    // Scale logic
                    if (transform.localScale.x + growthIncrement <= maxScale &&
                        transform.localScale.y + growthIncrement <= maxScale)
                    {
                        transform.localScale += new Vector3(growthIncrement, growthIncrement, 0);
                    }
                    else
                    {
                        transform.localScale = new Vector3(maxScale, maxScale, 1);
                    }

                    // Position adjustment logic with a height limit
                    if (transform.position.y + yPositionIncrement <= maxHeight)
                    {
                        transform.position += new Vector3(0, yPositionIncrement, 0);
                    }
                    else
                    {
                        transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
                    }

                    // Reset the drying out timer whenever the sprout grows
                    DryingOut dryingOutComponent = GetComponent<DryingOut>();
                    if (dryingOutComponent != null)
                    {
                        dryingOutComponent.ResetOnGrowth();
                    }

                    waterRenderer.enabled = false;
                    lastWaterTime = Time.time; // Update the last water time
                }
            }
        }
    }

    // Accessor method for lastWaterTime
    public float GetLastWaterTime()
    {
        return lastWaterTime;
    }

    // Accessor method for waterCooldown
    public float GetWaterCooldown()
    {
        return waterCooldown;
    }
}
