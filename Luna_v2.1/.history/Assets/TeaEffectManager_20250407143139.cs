using System.Collections.Generic;
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

    // Example: Refill 25% of Luna’s light
    LunaLightManager light = luna.GetComponent<LunaLightManager>();
    if (light != null)
    {
        light.RefillLight(0.25f);
    }

    // Add more effects here later!
}


    }
