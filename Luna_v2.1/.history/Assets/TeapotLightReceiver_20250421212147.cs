using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings (auto‑finds children if left blank)")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    [Header("Spawn Settings")]
    [Tooltip("Where the brewed teacup should appear")]
    public Transform teacupSpawnPoint;

    // no inspector slot—this will be loaded at runtime
    private GameObject _teacupPrefab;

    private bool _isLit = false;
    private List<GameObject> _ingredients = new List<GameObject>();

    void Awake()
    {
        // auto‑find effects if you forgot to hook them up in the Inspector
        if (sparkleEffect == null)
            sparkleEffect = transform.Find("SparkleEffect")?.gameObject;
        if (brewingIndicatorIcon == null)
            brewingIndicatorIcon = transform.Find("BrewingIndicatorIcon")?.gameObject;

        // start with everything off
        if (sparkleEffect != null)        sparkleEffect.SetActive(false);
        if (brewingIndicatorIcon != null) brewingIndicatorIcon.SetActive(false);

        // load the Teacup prefab from Resources/Teacup.prefab
        _teacupPrefab = Resources.Load<GameObject>("Teacup");
        if (_teacupPrefab == null)
            Debug.LogError("🫖 Teacup prefab not found! Make sure Assets/Resources/Teacup.prefab exists.");

        if (teacupSpawnPoint == null)
        teacupSpawnPoint = transform.Find("TeacupSpawnPoint");

        if (teacupSpawnPoint == null)
        Debug.LogWarning("TeacupSpawnPoint child not found!  Make sure you have a child named exactly 'TeacupSpawnPoint'.");
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

    /// <summary> True once it's been lit. </summary>
    public bool HasLight => _isLit;

    /// <summary> Add a flower GameObject as an ingredient. </summary>
    public void AddIngredient(GameObject flower)
    {
        if (!_ingredients.Contains(flower))
            _ingredients.Add(flower);
    }

    /// <summary> True if one or more flowers have been added. </summary>
    public bool HasAnyIngredients() => _ingredients.Count > 0;

    /// <summary>
    /// Brew the tea: returns a teacup instance if ready, or null otherwise.
    /// </summary>
   public GameObject BrewTea()
    {
        if (!_isLit || _ingredients.Count == 0 || _teacupPrefab == null)
            return null;

        Vector3 spawnPos = (teacupSpawnPoint != null)
            ? teacupSpawnPoint.position
            : transform.position;

        var cup = Instantiate(_teacupPrefab, spawnPos, Quaternion.identity);
        ResetTeapot();
        return cup;
    }

    /// <summary>
    /// True if the teapot has been lit (regardless of ingredients).
    /// Used by UI buttons to enable the Brew‑button.
    /// </summary>
    public bool IsReadyToBrew() => _isLit;

    /// <summary> Clears light + ingredients and hides all indicators. </summary>
    public void ResetTeapot()
    {
        _isLit = false;
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
    }
}
