using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // For MMProgressBar

public class TeaEffectManager : MonoBehaviour
{
    private List<string> ingredients = new List<string>();

    public bool wasLightBrewed = false;
    public bool includesFlower = false;

    public void SetIngredients(List<string> flowerTypes, bool brewedWithLight)
    {
        ingredients = new List<string>(flowerTypes);
        wasLightBrewed = brewedWithLight;
        includesFlower = flowerTypes.Count > 0;

        Debug.Log("🍵 Teacup created with ingredients: " + string.Join(", ", ingredients));
        Debug.Log($"🌕 Brewed with light: {wasLightBrewed} | Includes flowers: {includesFlower}");
    }

    private void Start()
    {
        // Optional initialization logic
    }

    public void ApplyEffects(GameObject luna)
    {
        Debug.Log("🍵 TeaEffectManager: ApplyEffects() called on " + gameObject.name);

        // 1. Try to increase LightBar UI
        GameObject barObj = GameObject.Find("LightBar");
        if (barObj != null)
        {
            MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();
            if (lightBar != null)
            {
                float current = lightBar.BarProgress;
                float target = Mathf.Clamp01(current + GetLightRestoreAmount());
                lightBar.UpdateBar01(target);
                Debug.Log("🌕 TeaEffectManager: LightBar updated by tea.");
            }
            else
            {
                Debug.LogWarning("❗ LightBar object found, but MMProgressBar component is missing.");
            }
        }
        else
        {
            Debug.LogWarning("❗ Could not find LightBar in the scene.");
        }

        // 2. Refill Luna's internal light stat
        LunaLightManager light = luna.GetComponent<LunaLightManager>();
        if (light != null)
        {
            float restoreAmount = GetLightRestoreAmount();
            light.RefillLight(restoreAmount);
            Debug.Log($"✨ LunaLightManager: RefillLight({restoreAmount}) triggered.");
        }
        else
        {
            Debug.Log("⚠️ LunaLightManager component not found on Luna.");
        }
    }

    private float GetLightRestoreAmount()
    {
        if (wasLightBrewed && !includesFlower)
        {
            Debug.Log("💡 Pure light tea: full restore");
            return 0.25f;
        }
        else if (wasLightBrewed && includesFlower)
        {
            Debug.Log("🌺✨ Mixed tea: partial restore");
            return 0.15f;
        }
        else if (!wasLightBrewed && includesFlower)
        {
            Debug.Log("🌸 Flower-only tea: no light restored");
            return 0f;
        }
        else
        {
            Debug.Log("⚠️ Unknown tea type");
            return 0f;
        }
    }
}
