using UnityEngine;

public class FlowerBehavior : MonoBehaviour
{
    private SproutManager sproutManager;

    private void Start()
    {
        // Find the SproutManager in the scene
        sproutManager = FindObjectOfType<SproutManager>();
        if (sproutManager == null)
        {
            Debug.LogError("SproutManager not found in the scene!");
            return;
        }

        // Register this flower with the manager
        sproutManager.RegisterFlower(gameObject);
    }

    private void OnDestroy()
    {
        // Unregister this flower from the manager
        if (sproutManager != null)
        {
            sproutManager.UnregisterFlower(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify the manager to activate this flower
            sproutManager.ActivateFlower(gameObject);
        }
    }
}
