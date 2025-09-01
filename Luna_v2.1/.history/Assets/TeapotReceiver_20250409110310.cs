using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("Brewing Components")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    [Header("Visual Effects")]
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    [Header("State Tracking")]
    public bool hasLight = false;
    public List<string> teaIngredients = new List<string>();

    // Icon containers
    private GameObject brewIcon;
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Find TeacupBrewIcon and all ingredient icons under IconDisplay
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "TeacupBrewIcon")
            {
                brewIcon = child.gameObject;
                brewIcon.SetActive(false);
                Debug.Log("✅ Found TeacupBrewIcon.");
            }
            else if (child.name.EndsWith("IconTeapot"))
            {
                ingredientIcons[child.name.Replace("IconTeapot", "")] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }

        if (brewIcon == null)
        {
            Debug.LogWarning("⚠️ TeacupBrewIcon not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle light
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        // Handle flower drop
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null && holder.HasFlower())
        {
            string flowerType = holder.currentFlowerType;
            AddIngredient(flowerType);
            ShowIngredientIcon(flowerType);
            holder.DropFlower();
        }
    }

    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon != null) brewIcon.SetActive(true);

        Debug.Log("✨ Teapot charged with light and ready to brew!");
    }

    public void AddIngredient(string flowerType)
    {
        teaIngredients.Add(flowerType);
        Debug.Log("🌸 Added flower: " + flowerType);
    }

    private void ShowIngredientIcon(string flowerType)
    {
        // Try to show the corresponding icon if it exists
        if (ingredientIcons.TryGetValue(flowerType, out GameObject icon))
        {
            icon.SetActive(true);
            Debug.Log($"🪻 Activated icon for {flowerType}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No icon mapped for flower type: {flowerType}");
        }
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Can't brew without light.");
            return null;
        }

        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.SetIngredients(teaIngredients, brewedWithLight: true);
        }

        // Reset visuals and state
        teaIngredients.Clear();
        hasLight = false;

        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon != null) brewIcon.SetActive(false);

        foreach (var icon in ingredientIcons.Values)
        {
            icon.SetActive(false);
        }

        return teacup;
    }
}
