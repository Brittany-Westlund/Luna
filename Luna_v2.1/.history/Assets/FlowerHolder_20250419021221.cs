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
        if (heldFlower != null) return;  // one at a time

        heldFlower = flower;

        // 1) Remember its original localScale
        Vector3 originalScale = flower.transform.localScale;

        // 2) Tell it it's held
        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
            sprout.isHeld = true;

        // 3) Parent it under holdPoint WITHOUT letting Unity recalc its scale
        //    (worldPositionStays=false keeps local values intact)
        flower.transform.SetParent(holdPoint, false);

        // 4) Snap into place
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;
        flower.transform.localScale    = originalScale;

        // 5) Disable its collider so it doesn’t block you
        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
            sprout.isHeld = false;

        // Unparent (worldPositionStays=true by default),
        // so its world position/scale remain the same.
        heldFlower.transform.SetParent(null);

        // Re‑enable its collider
        if (heldFlower.TryGetComponent<Collider2D>(out var col))
            col.enabled = true;

        heldFlower = null;
    }
}
