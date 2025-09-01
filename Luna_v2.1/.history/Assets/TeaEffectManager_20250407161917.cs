using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // For MMProgressBar

public class TeaEffectManager : MonoBehaviour
{
    private List<string> ingredients = new List<string>();
    private bool brewedWithLight = false;

    public void SetIngredients(List<string> flowerTypes, bool brewedWithLight)
    {
        ingredients = new List<string>(flowerTypes);
        this.brewedWithLight = brewedWithLight;

        Debug.Log("🍵 Teacup created with ingredients: " + string.Join(", ", ingredients));
        Debug.Log("🌕 Brewed with light: " + brewedWithLight);
    }

    public void ApplyEffects(GameObject luna)
    {
        Debug.Log("🍵 TeaEffectManager: ApplyEffects() called on " + gameObject.name);

        // Update LightBar in UI
        GameObject barObj = GameObject.Find("LightBar");
        if (barObj != null)
        {
            MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();
            if (lightBar != null)
            {
                float current = lightBar.BarProgress;
                float target = Mathf.Clamp01(current + 0.25f);
                lightBar.UpdateBar01(target);
                Debug.Log("🌕 Light bar increased by 25% from tea.");
            }
            else
            {
                Debug.LogWarning("⚠️ LightBar found but MMProgressBar is missing.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find LightBar GameObject in the scene.");
        }

        // Optionally update Luna's internal light meter
        LunaLightManager light = luna.GetComponent<LunaLightManager>();
        if (light != null)
        {
            light.RefillLight(0.25f);
            Debug.Log("✨ Luna's internal light increased via LunaLightManager.");
        }

        // Extra effects based on brew type (you can expand this!)
        if (brewedWithLight)
        {
            Debug.Log("🌟 This tea was brewed with light! Consider a bonus effect here.");
        }
    }
}
