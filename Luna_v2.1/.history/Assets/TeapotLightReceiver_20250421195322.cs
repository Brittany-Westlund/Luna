// TeapotLightReceiver.cs
using System.Collections.Generic;
using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings (auto‑finds children if left blank)")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    bool       _isLit = false;
    List<GameObject> _ingredients = new List<GameObject>();

    void Awake()
    {
        // if you forgot to assign these in‑editor, try to find them by name
        if (sparkleEffect == null)
            sparkleEffect = transform.Find("SparkleEffect")?.gameObject;
        if (brewingIndicatorIcon == null)
            brewingIndicatorIcon = transform.Find("BrewingIndicatorIcon")?.gameObject;

        // make sure they start off
        if (sparkleEffect != null)          sparkleEffect.SetActive(false);
        if (brewingIndicatorIcon != null)   brewingIndicatorIcon.SetActive(false);
    }

    /// <summary>Called by the wand when you hit Q on a teapot</summary>
    public void ActivateBrewReadyState()
    {
        if (_isLit) return;
        _isLit = true;
        sparkleEffect?.SetActive(true);
        brewingIndicatorIcon?.SetActive(true);
        Debug.Log("🫖 Teapot is ready to brew!");
    }

    /// <summary>True once it’s been lit</summary>
    public bool hasLight => _isLit;

    /// <summary>Add a flower in</summary>
    public void AddIngredient(GameObject flower)
    {
        _ingredients.Add(flower);
    }

    /// <summary>True if one or more flowers have been added</summary>
    public bool HasAnyIngredients() => _ingredients.Count > 0;

    /// <summary>Clear everything back to empty/unlit</summary>
    public void ResetTeapot()
    {
        _isLit = false;
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
    }

    /// <summary>Call when you actually brew tea.  Returns your teacup GameObject.</summary>
    public GameObject BrewTea()
    {
        if (!_isLit || _ingredients.Count == 0) return null;
        // … your tea‑cup creation logic here …
        GameObject cup = /* Instantiate your teacup prefab */;
        ResetTeapot();
        return cup;
    }
}
