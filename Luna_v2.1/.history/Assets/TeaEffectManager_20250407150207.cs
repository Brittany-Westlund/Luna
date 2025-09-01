using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // For MMProgressBar

public class TeaEffectManager : MonoBehaviour
{
    private List<string> ingredients = new List<string>();

    public void SetIngredients(List<string> flowerTypes)
    {
        ingredients = new List<string>(flowerTypes);
        Debug.Log("🍵 Teacup created with ingredients: " + string.Join(", ", ingredients));
    }

    private void Start()
    {
        // You can add initialization logic here if needed
    }

    public void ApplyEffects(GameObject luna)
    {
        Debug.Log("🍵 TeaEffectManager: ApplyEffects() called on " + gameObject.name);

        // First try to find the LightBar in the scene
        GameObject barObj = GameObject.Find("LightBar"); // Make sure the name matches your scene object

        if (barObj != null)
        {
            MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();

            if (lightBar != null)
            {
                float current = lightBar.BarProgress;
                float target = Mathf.Clamp01(current + 0.25f); // Increase by 25%
                lightBar.UpdateBar01(target);
                Debug.Log("🌕 TeaEffectManager: Light bar increased by 25%!");
            }
            else
            {
                Debug.LogWarning("❗ TeaEffectManager: LightBar object found, but MMProgressBar component is missing.");
            }
        }
        else
        {
            Debug.LogWarning("❗ TeaEffectManager: Could not find LightBar object in the scene.");
        }

        // Optional: also trigger Luna's internal light logic
        LunaLightManager light = luna.GetComponent<LunaLightManager>();
        if (light != null)
        {
            light.RefillLight(0.25f);
            Debug.Log("✨ LunaLightManager: RefillLight() also triggered.");
        }
        else
        {
            Debug.Log("⚠️ LunaLightManager component not found on Luna.");
        }
    }
}
