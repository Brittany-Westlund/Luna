using System.Collections.Generic;
using UnityEngine;

public class TeaEffectManager : MonoBehaviour
{
    public List<string> ingredients = new List<string>();

    public void SetIngredients(List<string> newIngredients)
    {
        ingredients.Clear();
        ingredients.AddRange(newIngredients);

        // TODO: Apply tea effects here based on ingredients
        Debug.Log("🍵 Teacup created with ingredients: " + string.Join(", ", ingredients));
    }
}
