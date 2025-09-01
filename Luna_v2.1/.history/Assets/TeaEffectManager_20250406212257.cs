using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

public class TeaEffectManager : MonoBehaviour
{
    [Header("Ingredient Info")]
    public List<string> ingredients = new List<string>();

    [Header("Visual Feedback")]
    public GameObject heartIcon;
    public GameObject elixirIcon;

    [Header("Effect Settings")]
    public float butterflySpeedDuration = 60f;
    public float fairyAccessDuration = 120f;

    private bool isElixir = false;
    private bool isTasty = false;

    void Start()
    {
        ProcessIngredients();
    }

    public void SetIngredients(List<string> newIngredients)
    {
        ingredients = new List<string>(newIngredients);
        ProcessIngredients();
    }

    private void ProcessIngredients()
    {
        if (ingredients == null || ingredients.Count == 0) return;

        isTasty = ingredients.Count > 1 || (ingredients.Count == 1 && ingredients[0] != "Light");
        isElixir = ingredients.Contains("Moonflower") &&
                   ingredients.Contains("Foxglove") &&
                   ingredients.Contains("Iris") &&
                   ingredients.Contains("Goldenrod") &&
                   ingredients.Contains("Buttercup") &&
                   ingredients.Contains("TeaRose");

        if (heartIcon != null) heartIcon.SetActive(isTasty);
        if (elixirIcon != null) elixirIcon.SetActive(isElixir);
    }

    public void ApplyEffects(GameObject luna, GameObject butterfly)
    {
        // Always refill light
        LunaLightManager lightManager = luna.GetComponent<LunaLightManager>();
        if (lightManager != null)
        {
            lightManager.FillLight();
        }

        // Apply tasty heart effect (purely visual)
        if (isTasty)
        {
            HeartEffectManager heartEffect = luna.GetComponent<HeartEffectManager>();
            if (heartEffect != null)
            {
                heartEffect.ShowHeart();
            }
        }

        foreach (string ingredient in ingredients)
        {
            switch (ingredient)
            {
                case "TeaRose":
                    ButterflyBuffManager buff = butterfly.GetComponent<ButterflyBuffManager>();
                    if (buff != null) buff.ApplySpeedBuff(butterflySpeedDuration);
                    break;

                case "Buttercup":
                    ButterflyFatigueManager fatigue = butterfly.GetComponent<ButterflyFatigueManager>();
                    if (fatigue != null) fatigue.ReduceFatigue();
                    break;

                case "Goldenrod":
                    LunaHealthManager health = luna.GetComponent<LunaHealthManager>();
                    if (health != null) health.RefillHealth();
                    break;

                case "Foxglove":
                    FairyPollenStatus pollen = luna.GetComponent<FairyPollenStatus>();
                    if (pollen != null) pollen.ApplyFairyAccess(fairyAccessDuration);
                    break;
            }
        }

        // Optional: Handle special elixir behavior here
        if (isElixir)
        {
            Debug.Log("Elixir consumed! This can now unlock the final fairy barrier.");
        }
    }
}
