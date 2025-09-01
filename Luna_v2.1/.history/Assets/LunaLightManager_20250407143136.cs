using UnityEngine;
using MoreMountains.Tools; // for MMProgressBar

public class LunaLightManager : MonoBehaviour
{
    public MMProgressBar lightBar;

    public void RefillLight(float amountPercent)
    {
        if (lightBar != null)
        {
            float current = lightBar.BarProgress;
            float updated = Mathf.Clamp01(current + amountPercent);
            lightBar.UpdateBar01(updated);
            Debug.Log("🔆 Luna’s light refilled by " + amountPercent * 100 + "%");
        }
    }
}
