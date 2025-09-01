using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    [Header("Spawn Settings")]
    [Tooltip("Where the teacup should appear when brewed")]
    public Transform teacupSpawnPoint;

    // loaded from Resources/Teacup.prefab or assign in editor
    private GameObject _teacupPrefab;

    private bool _isLit = false;
    // we’ll keep the list around if you want to show icons later, 
    // but it no longer gates brewing:
    private List<GameObject> _ingredients = new List<GameObject>();

    void Awake()
    {
        // auto‐find effects
        sparkleEffect = sparkleEffect ?? transform.Find("SparkleEffect")?.gameObject;
        brewingIndicatorIcon = brewingIndicatorIcon ?? transform.Find("BrewingIndicatorIcon")?.gameObject;

        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);

        // auto‐find spawn point
        teacupSpawnPoint = teacupSpawnPoint ?? transform.Find("TeacupSpawnPoint");
        if (teacupSpawnPoint == null)
            Debug.LogWarning("TeacupSpawnPoint not found!");

        // load cup
        _teacupPrefab = Resources.Load<GameObject>("Teacup");
        if (_teacupPrefab == null)
            Debug.LogError("Teacup prefab not found in Resources/Teacup.prefab!");
    }

    /// <summary> Called by the wand (Q) to light the pot. </summary>
    public void ActivateBrewReadyState()
    {
        if (_isLit) return;
        _isLit = true;
        sparkleEffect.SetActive(true);
        brewingIndicatorIcon.SetActive(true);
        Debug.Log("🫖 Teapot is ready to brew!");
    }

    /// <summary> True if it’s been lit. </summary>
    public bool HasLight => _isLit;

    /// <summary> Still tracks flowers if you want later—but no longer required. </summary>
    public void AddIngredient(GameObject flower)
    {
        if (!_ingredients.Contains(flower))
            _ingredients.Add(flower);
    }

    /// <summary> Brew the tea: now only requires light. </summary>
    public GameObject BrewTea()
    {
        Debug.Log($"BrewTea() → isLit={_isLit}, prefabLoaded={_teacupPrefab != null}");
        if (!_isLit || _teacupPrefab == null)
            return null;

        // choose spawn position
        Vector3 spawnPos = teacupSpawnPoint != null
            ? teacupSpawnPoint.position
            : transform.position;

        var cup = Instantiate(_teacupPrefab, spawnPos, Quaternion.identity);
        ResetTeapot();
        return cup;
    }

    /// <summary> Clears state + hides effects. </summary>
    public void ResetTeapot()
    {
        _isLit = false;
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
    }
}
