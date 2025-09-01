using UnityEngine;
using MoreMountains.Tools;

public class LunaLightManager : MonoBehaviour
{
    public MMProgressBar LightBar; // Assign this in the inspector!
    public float CurrentLight => LightBar != null ? LightBar.BarProgress : 0f;


    void Start()
{
    if (LightBar == null)
    {
        LightBar = FindObjectOfType<MMProgressBar>();
    }
}


    public void RefillLight(float amountPercent)
    {
        if (LightBar != null)
        {
            float current = LightBar.BarProgress;
            float updated = Mathf.Clamp01(current + amountPercent);
            LightBar.UpdateBar01(updated);
            Debug.Log("🔆 Luna’s light bar refilled by " + (amountPercent * 100f) + "%");
        }
        else
        {
            Debug.LogWarning("Light bar is missing from LunaLightManager!");
        }
    }
}
