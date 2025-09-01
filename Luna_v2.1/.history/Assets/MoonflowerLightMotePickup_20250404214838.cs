using UnityEngine;
using MoreMountains.Tools;

public class MoonflowerLightMotePickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float lightAmount = 0.1f;

    private MMProgressBar LightBar;

    private void Start()
    {
        // Try to find the MMProgressBar in the scene
        LightBar = FindObjectOfType<MMProgressBar>();

        if (LightBar == null)
        {
            Debug.LogWarning("MMProgressBar not found in scene by LightMotePickup.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LightBar != null)
        {
            float newValue = Mathf.Clamp01(LightBar.BarProgress + lightAmount);
            LightBar.UpdateBar01(newValue);
            Destroy(gameObject);
        }
    }
}
