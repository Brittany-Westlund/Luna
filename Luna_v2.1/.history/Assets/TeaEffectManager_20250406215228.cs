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
    }
