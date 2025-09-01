using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [HideInInspector] public Transform holdPoint;
    private GameObject heldFlower;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;

    public string CurrentFlowerType
    {
        get
        {
            if (heldFlower == null) return null;
            var pickup = heldFlower.GetComponent<FlowerPickup>();
            return (pickup != null) ? pickup.flowerType : null;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (heldFlower != null) return;  // only one at a time

        // Tell the sprout it’s held
        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        // Parent *with* worldPositionStays = true (default),
        // so the flower’s world‐scale (and world‐rotation) never change.
        flower.transform.SetParent(holdPoint);

        // Snap it into place at the holdPoint
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;

        // Disable its collider so it can’t interfere
        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        heldFlower = flower;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        // Tell the sprout it’s no longer held
        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        // Unparent with worldPositionStays = true (default),
        // so the world‐scale/rotation remain exactly as before.
        heldFlower.transform.SetParent(null);

        // Re‐enable its collider
        if (heldFlower.TryGetComponent<Collider2D>(out var col))
            col.enabled = true;

        heldFlower = null;
    }
}
