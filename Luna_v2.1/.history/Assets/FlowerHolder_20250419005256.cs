// FlowerHolder.cs
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
            return pickup != null ? pickup.flowerType : null;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (heldFlower != null) return;

        heldFlower = flower;
        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
            sprout.isHeld = true;

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
            sprout.isHeld = false;

        heldFlower.transform.SetParent(null);

        if (heldFlower.TryGetComponent<Collider2D>(out var col))
            col.enabled = true;

        heldFlower = null;
    }
}
