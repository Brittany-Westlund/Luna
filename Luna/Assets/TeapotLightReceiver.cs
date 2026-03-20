using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    [Tooltip("If false, this teapot does not require light to brew.")]
    public bool requireLight = true;

    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    [Header("Spawn Settings")]
    [Tooltip("Where the teacup should appear when brewed")]
    public Transform teacupSpawnPoint;

    [Header("Hint Icon")]
    public GameObject lightHintIcon;

    [Header("Audio")]
    public AudioSource audioSource; // adding light sfx

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

        teacupSpawnPoint = teacupSpawnPoint ?? transform.Find("TeacupSpawnPoint");
        if (teacupSpawnPoint == null)
            Debug.LogWarning("TeacupSpawnPoint not found!");

        _teacupPrefab = Resources.Load<GameObject>("Teacup");
        if (_teacupPrefab == null)
            Debug.LogError("Teacup prefab not found in Resources/Teacup.prefab!");

        // If light is not required, treat the teapot as already satisfying the light condition.
        if (!requireLight)
        {
            _isLit = true;

            var receiver = GetComponent<TeapotReceiver>();
            if (receiver != null)
                receiver.hasLight = true;
        }

        if (lightHintIcon != null)
            lightHintIcon.SetActive(requireLight && !_isLit);
    }

    void Update()
    {
        // Show hint only if light is required and not yet lit
        if (lightHintIcon != null)
            lightHintIcon.SetActive(requireLight && !_isLit);
    }

    public void ActivateBrewReadyState()
    {
        if (!requireLight)
        {
            // If light isn't required, keep it logically satisfied and skip lighting visuals/audio.
            _isLit = true;

            var receiverNoLight = GetComponent<TeapotReceiver>();
            if (receiverNoLight != null)
                receiverNoLight.hasLight = true;

            lightHintIcon?.SetActive(false);
            return;
        }

        if (_isLit) return;

        _isLit = true;
        sparkleEffect?.SetActive(true);
        brewingIndicatorIcon?.SetActive(true);
        lightHintIcon?.SetActive(false); // turn off hint when lit
        Debug.Log("🫖 Teapot is ready to brew!");

        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
            receiver.hasLight = true;

        // --- Play "teapot lit" sound here ---
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }

    public bool HasLight => !requireLight || _isLit;

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
        return !requireLight || _isLit;
    }

    public GameObject BrewTea()
    {
        // Forward to TeapotReceiver if available, to handle points/flowers!
        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
        {
            return receiver.BrewTea();
        }

        // If no receiver, fallback to old logic
        Debug.LogWarning("No TeapotReceiver found, using legacy brew logic (no flower handling).");
        Debug.Log($"BrewTea() → isLit={_isLit}, prefabLoaded={_teacupPrefab != null}");

        if ((requireLight && !_isLit) || _teacupPrefab == null)
        {
            ResetTeapot();
            return null;
        }

        Vector3 spawnPos = teacupSpawnPoint != null
            ? teacupSpawnPoint.position
            : transform.position;

        var cup = Instantiate(_teacupPrefab, spawnPos, Quaternion.identity);

        // ---- PLAY BREW SOUND ON THE CUP ----
        var sfx = cup.GetComponent<TeacupBrewSFX>();
        if (sfx != null)
        {
            Debug.Log("Calling PlayBrewSound() on Teacup");
            sfx.PlayBrewSound();
        }
        else
        {
            Debug.LogWarning("TeacupBrewSFX not found on new Teacup!");
        }

        ResetTeapot();
        return cup;
    }

    public void ResetTeapot()
    {
        _isLit = !requireLight; // stay satisfied if light is not required
        _ingredients.Clear();
        sparkleEffect?.SetActive(false);
        brewingIndicatorIcon?.SetActive(false);
        lightHintIcon?.SetActive(requireLight && !_isLit);

        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
            receiver.hasLight = !requireLight;
    }
}