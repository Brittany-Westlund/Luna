using UnityEngine;

public class AidingSprouts : MonoBehaviour
{
    private SpriteRenderer sproutRenderer;
    public float growthIncrement = 0.1f;
    public float yPositionIncrement = 0.05f;
    public float maxScale = 0.2215f; // Maximum scale allowed for the sprout
    public float maxHeight = 1.098f; // Maximum height the sprout can move to
    public float lastSporeTime = -30f; // Track the last spore interaction time
    public float sporeCooldown = 30f; // Cooldown in seconds

    void Start()
    {
        sproutRenderer = GetComponent<SpriteRenderer>();
        if (sproutRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on sprout object!");
        }
    }

    public void Grow()
    {
        Debug.Log($"Grow method called on sprout: {gameObject.name}");

        // Ensure the sprout is visible
        if (!sproutRenderer.enabled)
        {
            sproutRenderer.enabled = true;
        }

        // Scale logic
        if (transform.localScale.x + growthIncrement <= maxScale &&
            transform.localScale.y + growthIncrement <= maxScale)
        {
            transform.localScale += new Vector3(growthIncrement, growthIncrement, 0);
            Debug.Log("Sprout scaled up.");
        }
        else
        {
            transform.localScale = new Vector3(maxScale, maxScale, 1);
            Debug.Log("Sprout reached max scale.");
        }

        // Position adjustment logic with a height limit
        if (transform.position.y + yPositionIncrement <= maxHeight)
        {
            transform.position += new Vector3(0, yPositionIncrement, 0);
            Debug.Log("Sprout moved upward.");
        }
        else
        {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
            Debug.Log("Sprout reached max height.");
        }

        // Reset drying out timer
        DryingOut dryingOutComponent = GetComponent<DryingOut>();
        if (dryingOutComponent != null)
        {
            dryingOutComponent.ResetOnGrowth();
            Debug.Log("DryingOut timer reset.");
        }

        lastSporeTime = Time.time; // Update the last spore interaction time
    }

    // Debugging helpers
    public float GetLastSporeTime()
    {
        return lastSporeTime;
    }

    public float GetSporeCooldown()
    {
        return sporeCooldown;
    }
}
