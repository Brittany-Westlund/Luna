using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [Header("Hold Point for Picked-Up Flowers")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string currentFlowerType = "Unknown";

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string CurrentFlowerType => currentFlowerType;

    public void PickUpFlower(GameObject flower)
    {
        heldFlower = flower;

        // Update sprout state
        if (flower.TryGetComponent<SproutAndLightManager>(out var sprout))
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        // Parent it under the hold point
        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        // Disable its collider so it won't re-trigger pickups
        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        flower.SetActive(true);

        // Cache the flowerType from the FlowerPickup component
        if (flower.TryGetComponent<FlowerPickup>(out var meta) &&
            !string.IsNullOrEmpty(meta.flowerType))
        {
            currentFlowerType = meta.flowerType;
        }
        else
        {
            currentFlowerType = "Unknown";
        }
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        // Unparent and reset state
        heldFlower.transform.SetParent(null);
        heldFlower = null;
        currentFlowerType = "Unknown";
    }
}
