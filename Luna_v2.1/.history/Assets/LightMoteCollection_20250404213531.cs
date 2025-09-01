using UnityEngine;
using MoreMountains.Tools; // Required for MMProgressBar

public class MoonflowerLightMotePickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float lightAmount = 0.1f; // 10%

    public MMProgressBar lightBar; // Assign this in the Inspector or via a manager

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (lightBar != null)
            {
                float newValue = Mathf.Clamp01(lightBar.BarProgress + lightAmount);
                lightBar.UpdateBar01(newValue);
            }
            else
            {
                Debug.LogWarning("LightBar reference not set on MoonflowerLightMotePickup!");
            }

            Destroy(gameObject);
        }
    }
}
