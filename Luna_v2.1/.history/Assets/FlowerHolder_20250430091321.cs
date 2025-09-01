using System;
using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [HideInInspector] public Transform holdPoint;
    private GameObject heldFlower;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public AudioSource pickupSFXSource;
    public AudioSource plantSFXSource;

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
        if (heldFlower != null) return;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        PlayPickupSFX();

        flower.transform.SetParent(null); // 💥 unparent it first
        flower.transform.SetParent(holdPoint, false); // resets local transform
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;
        flower.transform.localScale = Vector3.one;

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

        PlayPlantSFX();
    }
    public void PlayPickupSFX()
    {
        Debug.Log($"PlayPickupSFX called on {gameObject.name}");
        if (pickupSFXSource)
        {
            pickupSFXSource.Play();
        }
    }

    public void PlayPlantSFX()
    {
        Debug.Log($"PlayPlantSFX called on {gameObject.name}");
        if (plantSFXSource)
        {
            plantSFXSource.Play();
        }
    }
}
