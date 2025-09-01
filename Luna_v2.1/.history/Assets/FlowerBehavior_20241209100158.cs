using UnityEngine;

public class FlowerBehavior : MonoBehaviour
{
    private SproutAndLightManager sproutManager;

    private void Start()
    {
        // Find the SproutAndLightManager in the scene
        sproutManager = FindObjectOfType<SproutAndLightManager>();
        if (sproutManager == null)
        {
            Debug.LogError("SproutAndLightManager not found in the scene!");
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
