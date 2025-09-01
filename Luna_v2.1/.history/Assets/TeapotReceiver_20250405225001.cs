// TeapotReceiver.cs
using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    public List<string> teaIngredients = new List<string>();

    [Header("Icon Display")]
    public Transform iconDisplayParent;
    public GameObject iconPrefab;

    [Header("Teacup Settings")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    [Header("Light & Sparkle")]
    public bool hasLight = false;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;
    public KeyCode brewKey = KeyCode.B;

    [Header("Brew Icon")]
    [SerializeField] private GameObject brewIcon; // 💡 Assign this directly in prefab!

    private void Start()
    {
        if (brewIcon != null)
        {
            brewIcon.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ Brew icon not assigned in Inspector!");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
        }

        FlowerHolder holder = other.GetComponent<FlowerHolder>();
        if (holder != null && holder.HasFlower())
        {
            AddIngredient(holder.currentFlowerType);
            CreateIngredientIcon(holder.currentFlowerType);
            holder.DropFlower();
        }
    }

    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(true);
        }

        if (teapotAnimator != null)
        {
            teapotAnimator.SetTrigger("Pulse");
        }

        UpdateBrewIcon();
        Debug.Log("✨ Teapot is charged with light!");
    }

    private void Update()
    {
        if (hasLight && Input.GetKeyDown(brewKey))
        {
            BrewTea();
        }
    }

    public void AddIngredient(string flowerType)
    {
        teaIngredients.Add(flowerType);
        Debug.Log("🫖 Added flower to teapot: " + flowerType);
        UpdateBrewIcon();
    }

    public void CreateIngredientIcon(string flowerType)
    {
        GameObject icon = Instantiate(iconPrefab, iconDisplayParent);
        // Optional: Set icon appearance based on flowerType
    }

    public void BrewTea()
    {
        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effectManager = teacup.GetComponent<TeaEffectManager>();
        if (effectManager != null)
        {
            effectManager.SetIngredients(teaIngredients); // Can be empty!
        }

        teaIngredients.Clear();

        foreach (Transform child in iconDisplayParent)
        {
            Destroy(child.gameObject);
        }

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }

        if (brewIcon != null)
        {
            brewIcon.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private void UpdateBrewIcon()
    {
        if (brewIcon != null)
        {
            brewIcon.SetActive(hasLight); // Show only if light is present
        }
    }
}
