using UnityEngine;
using System.Collections.Generic;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    [Tooltip("If false, this teapot does not require light to brew.")]
    public bool requireLight = true;

    [Header("Visual References")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;
    public GameObject lightHintIcon;

    [Header("Audio")]
    public AudioSource audioSource;

    private bool _isLit = false;
    private readonly List<GameObject> _ingredients = new List<GameObject>();

    public bool HasLight => !requireLight || _isLit;

    private void Awake()
    {
        sparkleEffect = sparkleEffect ?? transform.Find("SparkleEffect")?.gameObject;
        brewingIndicatorIcon = brewingIndicatorIcon ?? transform.Find("BrewingIndicatorIcon")?.gameObject;
        lightHintIcon = lightHintIcon ?? transform.Find("LightHintIcon")?.gameObject;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewingIndicatorIcon != null)
            brewingIndicatorIcon.SetActive(false);

        if (!requireLight)
        {
            _isLit = true;

            var receiver = GetComponent<TeapotReceiver>();
            if (receiver != null)
                receiver.hasLight = true;
        }

        RefreshHintState();
    }

    private void Update()
    {
        RefreshHintState();
    }

    private void RefreshHintState()
    {
        if (lightHintIcon != null)
            lightHintIcon.SetActive(requireLight && !_isLit);
    }

    public void ActivateBrewReadyState()
    {
        if (!requireLight)
        {
            _isLit = true;

            var receiverNoLight = GetComponent<TeapotReceiver>();
            if (receiverNoLight != null)
                receiverNoLight.hasLight = true;

            RefreshHintState();
            return;
        }

        if (_isLit)
            return;

        _isLit = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (brewingIndicatorIcon != null)
            brewingIndicatorIcon.SetActive(true);

        RefreshHintState();

        Debug.Log("🫖 Teapot is ready to brew!");

        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
            receiver.hasLight = true;

        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }

    public void AddIngredient(GameObject flower)
    {
        if (flower != null && !_ingredients.Contains(flower))
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
        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
            return receiver.BrewTea();

        Debug.LogWarning("[TeapotLightReceiver] No TeapotReceiver found; cannot brew.");
        return null;
    }

    public void ResetTeapot()
    {
        _isLit = !requireLight;
        _ingredients.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewingIndicatorIcon != null)
            brewingIndicatorIcon.SetActive(false);

        RefreshHintState();

        var receiver = GetComponent<TeapotReceiver>();
        if (receiver != null)
            receiver.hasLight = !requireLight;
    }
}