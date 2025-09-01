using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings (auto‑finds children if left blank)")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;
    [Tooltip("Prefab of the teacup to spawn when brewing")]
    private GameObject teacupPrefab;

    bool _isLit = false;
    List<GameObject> _ingredients = new List<GameObject>();

    void Awake()
    {
        // if forgot to assign these in‑editor, try to find them by name
        if (sparkleEffect == null)
            sparkleEffect = transform.Find("SparkleEffect")?.gameObject;
        if (brewingIndicatorIcon == null)
            brewingIndicatorIcon = transform.Find("BrewingIndicatorIcon")?.gameObject;

        // make sure they start off
        if (sparkleEffect != null)        sparkleEffect.SetActive(false);
        if (brewingIndicatorIcon != null) brewingIndicatorIcon.SetActive(false);

        teacupPrefab = Resources.Load<GameObject>("Teacup");

    }

    /// <summary>
    /// Called by the wand (on Q) to light the teapot.
    /// </summary>
    public void ActivateBrewReadyState()
    {
        if (_isLit) return;
        _isLit = true;
        sparkleEffect?.SetActive(true);
        brewingIndicatorIcon?.SetActive(true);
        Debug.Log("🫖 Teapot is ready to brew!");
    }

    /// <summary>
    /// True once it’s been lit.
    /// </summary>
    public bool HasLight => _isLit;

    /// <summary>
    /// Add a flower GameObject as an ingredient.
    /// </summary>
    public void AddIngredient(GameObject flower)
    {
        if (!_ingredients.Contains(flower))
            _ingredients.Add(flower);
    }

    /// <summary>
    /// True if one or more flowers have been added.
    /// </summary>
    public bool HasAnyIngredients() => _ingredients.Count > 0;

    /// <summary>
    /// Brew the tea: returns a teacup instance if ready, or null otherwise.
    /// </summary>
    public GameObject BrewTea()
    {
        if (!_isLit || _ingredients.Count == 0 || teacupPrefab == null)
            return null;

        var cup = Instantiate(teacupPrefab, transform.position, Quaternion.identity);
        ResetTeapot();
        return cup;
    }

    /// <summary>
    /// True if the teapot has been lit (regardless of ingredients).
    /// Used by UI buttons to enable the Brew‑button.
    /// </summary>
    public bool IsReadyToBrew() => _isLit;

    /// <summary>
    /// Clears light + ingredients and hides all indicators.
    /// </summary>
    public void ResetTeapot()
    {
        _isLit = false;
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
    }
}
