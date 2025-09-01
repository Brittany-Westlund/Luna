// FlowerHolder.cs
using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [Header("Where picked‑up flowers live")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string currentFlowerType = "Unknown";

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string CurrentFlowerType => currentFlowerType;

    public void PickUpFlower(GameObject flower)
    {
        heldFlower = flower;

        if (flower.TryGetComponent<SproutAndLightManager>(out var sprout))
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        if (flower.TryGetComponent<FlowerPickup>(out var meta) &&
            !string.IsNullOrEmpty(meta.flowerType))
            currentFlowerType = meta.flowerType;
        else
            currentFlowerType = "Unknown";
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        heldFlower.transform.SetParent(null);
        heldFlower = null;
        currentFlowerType = "Unknown";
    }
}
