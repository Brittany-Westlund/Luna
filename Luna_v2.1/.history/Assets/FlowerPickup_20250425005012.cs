// FlowerPickup.cs
using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    [Tooltip("Set in Inspector to identify this flower’s type")]
    public string flowerType = "Unknown";
    
    [Header("Pickup SFX")]
    public AudioSource pickupSFXSource;
    public AudioClip pickupSFX;
    
    /// <summary>True if the sprout is currently held.</summary>
    public bool IsHeld
    {
        get
        {
            var spr = GetComponent<SproutAndLightManager>();
            return (spr != null) && spr.isHeld;
        }
    }

    /// <summary>True if the sprout is currently planted.</summary>
    public bool IsPlanted
    {
        get
        {
            var spr = GetComponent<SproutAndLightManager>();
            return (spr != null) && spr.isPlanted;
        }
    }

    /// <summary>If planted under a GardenSpot, returns that spot; otherwise null.</summary>
    public GardenSpot CurrentGardenSpot
    {
        get
        {
            if (transform.parent == null)
                return null;
            return transform.parent.GetComponent<GardenSpot>();
        }
    }

    /// Play the pickup sound for this flower.
    /// </summary>
    public void PlayPickupSFX()
    {
        if (pickupSFXSource && pickupSFX)
        {
            pickupSFXSource.PlayOneShot(pickupSFX);
        }
    }
}
