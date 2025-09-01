using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    [Tooltip("Unique identifier for this flower’s type (e.g. \"Chamomile\").")]
    public string flowerType = "Unknown";

    // NEW:
    [HideInInspector] public bool IsHeld    = false;
    [HideInInspector] public bool IsPlanted = false;
    [HideInInspector] public GardenSpot CurrentGardenSpot = null;
}
