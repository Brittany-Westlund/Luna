using UnityEngine;

public class AidingSprouts : MonoBehaviour
{
    private SpriteRenderer sproutRenderer;
    public float growthIncrement = 0.1f;
    public float yPositionIncrement = 0.05f;
    public float maxScale = 0.2215f; // Maximum scale allowed for the sprout
    public float maxHeight = -1.098f; // Maximum height the sprout can move to
    public float lastSporeTime = -30f; // Track the last spore interaction time
    public float sporeCooldown = 30f; // Cooldown in seconds

    void Start()
    {
        sproutRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            if (Time.time - lastSporeTime >= sporeCooldown) // Check cooldown
            {
                Grow(); // Use the Grow method
            }
        }
    }

    public void Grow()
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

        // Reset the drying out timer
        DryingOut dryingOutComponent = GetComponent<DryingOut>();
        if (dryingOutComponent != null)
        {
            dryingOutComponent.ResetOnGrowth();
        }

        lastSporeTime = Time.time; // Update the last spore interaction time
    }

    // Accessor method for lastSporeTime
    public float GetLastSporeTime()
    {
        return lastSporeTime;
    }

    // Accessor method for sporeCooldown
    public float GetSporeCooldown()
    {
        return sporeCooldown;
    }
}
