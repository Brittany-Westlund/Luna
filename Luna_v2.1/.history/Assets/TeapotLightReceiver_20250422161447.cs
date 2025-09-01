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

    [Header("Hint Icon")]
    public GameObject lightHintIcon;

    private GameObject _teacupPrefab;

    private bool _isLit = false;
    private List<GameObject> _ingredients = new List<GameObject>();

    void Awake()
    {
        sparkleEffect = sparkleEffect ?? transform.Find("SparkleEffect")?.gameObject;
        brewingIndicatorIcon = brewingIndicatorIcon ?? transform.Find("BrewingIndicatorIcon")?.gameObject;
        lightHintIcon = lightHintIcon ?? transform.Find("LightHintIcon")?.gameObject;

        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
        lightHintIcon?.SetActive(true); // will get turned off below if needed

        teacupSpawnPoint = teacupSpawnPoint ?? transform.Find("TeacupSpawnPoint");
        if (teacupSpawnPoint == null)
            Debug.LogWarning("TeacupSpawnPoint not found!");

        _teacupPrefab = Resources.Load<GameObject>("Teacup");
        if (_teacupPrefab == null)
            Debug.LogError("Teacup prefab not found in Resources/Teacup.prefab!");
    }

    void Update()
    {
        // Show hint only if not lit
        if (lightHintIcon != null)
            lightHintIcon.SetActive(!_isLit);
    }

    public void ActivateBrewReadyState()
    {
        if (_isLit) return;
        _isLit = true;
        sparkleEffect.SetActive(true);
        brewingIndicatorIcon.SetActive(true);
        lightHintIcon?.SetActive(false); // turn off hint when lit
        Debug.Log("🫖 Teapot is ready to brew!");
    }

    public bool HasLight => _isLit;

    public void AddIngredient(GameObject flower)
    {
        if (!_ingredients.Contains(flower))
            _ingredients.Add(flower);
    }

    public bool HasAnyIngredients()
    {
        return _ingredients.Count > 0;
    }

    public int GetIngredientCount()
    {
        var receiver = GetComponent<TeapotReceiver>();
        return receiver != null ? receiver.GetIngredientCount() : 0;
    }

    public bool IsReadyToBrew()
    {
        return _isLit;
    }

    public GameObject BrewTea()
    {
        Debug.Log($"BrewTea() → isLit={_isLit}, prefabLoaded={_teacupPrefab != null}");
        if (!_isLit || _teacupPrefab == null)
            return null;

        Vector3 spawnPos = teacupSpawnPoint != null
            ? teacupSpawnPoint.position
            : transform.position;

        var cup = Instantiate(_teacupPrefab, spawnPos, Quaternion.identity);
        ResetTeapot();
        return cup;
    }

    public void ResetTeapot()
    {
        _isLit = false;
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
        lightHintIcon?.SetActive(true);
    }
}


/* using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    [Header("Spawn Settings")]
    [Tooltip("Where the teacup should appear when brewed")]
    public Transform teacupSpawnPoint;
    [Header("Hint Icon")]
    public GameObject lightHintIcon;

    // loaded from Resources/Teacup.prefab or assign in editor
    private GameObject _teacupPrefab;

    private bool _isLit = false;
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

    /// <summary> Still tracks flowers if you want later—but no longer required for brewing. </summary>
    public void AddIngredient(GameObject flower)
    {
        if (!_ingredients.Contains(flower))
            _ingredients.Add(flower);
    }

    /// <summary>
    /// True if one or more flowers have been added.
    /// </summary>
    public bool HasAnyIngredients()
    {
        return _ingredients.Count > 0;
    }
    public int GetIngredientCount()
    {
        var receiver = GetComponent<TeapotReceiver>();
        return receiver != null ? receiver.GetIngredientCount() : 0;
    }


    /// <summary>
    /// True once the teapot has been lit (regardless of ingredients).
    /// Used by UI buttons to enable the Brew‑button.
    /// </summary>
    public bool IsReadyToBrew()
    {
        return _isLit;
    }

    /// <summary> Brew the tea: now only requires light. </summary>
    public GameObject BrewTea()
    {
        Debug.Log($"BrewTea() → isLit={_isLit}, prefabLoaded={_teacupPrefab != null}");
        if (!_isLit || _teacupPrefab == null)
            return null;

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
*/