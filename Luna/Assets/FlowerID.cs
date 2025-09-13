using UnityEngine;
using System;

[RequireComponent(typeof(FlowerPickup))]
public class FlowerID : MonoBehaviour
{
    [Tooltip("Unique ID for this flower instance. Auto-generated if empty.")]
    public string flowerID;

    private FlowerPickup pickup;

    void Awake()
    {
        pickup = GetComponent<FlowerPickup>();

        // Generate a persistent unique ID if not set
        if (string.IsNullOrEmpty(flowerID))
        {
            flowerID = Guid.NewGuid().ToString();
        }

        // Check global state: should this flower exist?
        if (FlowerGlobalState.Instance != null &&
            FlowerGlobalState.Instance.IsFlowerPicked(flowerID))
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Record in global state if Luna picked it (not if it just despawned)
        if (FlowerGlobalState.Instance != null && pickup != null)
        {
            // Only count as "picked" if it was held or planted before destroy
            if (pickup.IsHeld || pickup.IsPlanted)
            {
                FlowerGlobalState.Instance.MarkFlowerPicked(flowerID);
            }
        }
    }
}
