using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

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
        
        }

    public void ApplyEffects(GameObject luna)
{
    Debug.Log("🍵 TeaEffectManager: ApplyEffects() called on " + gameObject.name);

    // Find the LightBar GameObject directly by name
    GameObject barObj = GameObject.Find("LightBar");

    if (barObj != null)
    {
        MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();

        if (lightBar != null)
        {
            float current = lightBar.BarProgress;
            float updated = Mathf.Clamp01(current + 0.25f); // 25% light boost
            lightBar.UpdateBar01(updated);
            Debug.Log("🍵 Luna drank tea — light bar increased!");
        }
        else
        {
            Debug.LogWarning("⚠️ Found LightBar GameObject but MMProgressBar is missing!");
        }
    }
    else
    {
        Debug.LogWarning("⚠️ LightBar GameObject not found in scene.");
        Debug.Log("🍵 ApplyEffects() was called");

    }

    // Add more effects here later!
}



    }
