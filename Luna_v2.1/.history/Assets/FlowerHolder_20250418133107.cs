// FlowerHolder.cs
using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;

    public void PickUpFlower(GameObject flower)
    {
        // **Guard against picking up if you already have one**
        if (heldFlower != null) return;

        heldFlower = flower;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld    = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;
        heldFlower.transform.SetParent(null);
        heldFlower = null;
    }
}
