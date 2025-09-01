using UnityEngine;
using MoreMountains.Tools;

public class LunaLightManager : MonoBehaviour
{
    public MMProgressBar lightBar; // Assign this in the inspector!

    void Start()
{
    if (lightBar == null)
    {
        lightBar = FindObjectOfType<MMProgressBar>();
    }
}


    public void RefillLight(float amountPercent)
    {
        if (lightBar != null)
        {
            float current = lightBar.BarProgress;
            float updated = Mathf.Clamp01(current + amountPercent);
            lightBar.UpdateBar01(updated);
            Debug.Log("🔆 Luna’s light bar refilled by " + (amountPercent * 100f) + "%");
        }
        else
        {
            Debug.LogWarning("Light bar is missing from LunaLightManager!");
        }
    }
}
