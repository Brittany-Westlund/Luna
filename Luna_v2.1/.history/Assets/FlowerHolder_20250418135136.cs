// FlowerHolder.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FlowerHolder : MonoBehaviour
{
    [Header("Where to hold a picked‑up flower")]
    public Transform holdPoint;

    private GameObject heldFlower;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;

    // Back‑compat for scripts expecting this
    public string CurrentFlowerType
    {
        get
        {
            if (heldFlower == null) return null;
            var fp = heldFlower.GetComponent<FlowerPickup>();
            return fp != null ? fp.flowerType : null;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (heldFlower != null) return;

        heldFlower = flower;
        var pickup = flower.GetComponent<FlowerPickup>();
        if (pickup != null)
        {
            pickup.IsHeld    = true;
            pickup.IsPlanted = false;
            pickup.CurrentGardenSpot = null;
        }

        // <-- preserve world scale/position
        flower.transform.SetParent(holdPoint, true);
        flower.transform.localPosition = Vector3.zero;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        var col = heldFlower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        heldFlower.transform.SetParent(null, true);
        heldFlower = null;
    }
}
