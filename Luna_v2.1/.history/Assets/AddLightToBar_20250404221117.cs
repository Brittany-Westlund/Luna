using UnityEngine;
using MoreMountains.Tools;

public class AddLightToBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float lightAmount = 0.1f;
    private MMProgressBar lightBar;

    private void Start()
    {
        // Automatically find the MMProgressBar in the scene
        lightBar = FindObjectOfType<MMProgressBar>();

        if (lightBar == null)
        {
            Debug.LogWarning("AddLightToBar: MMProgressBar not found in scene.");
        }
    }

    public void AddLight()
    {
        if (lightBar != null)
        {
            float newValue = Mathf.Clamp01(lightBar.BarProgress + lightAmount);
            lightBar.UpdateBar(newValue, 0f, 1f); // 👈 Fix is here
        }
    }
}
