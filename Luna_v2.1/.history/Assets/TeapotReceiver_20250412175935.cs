using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    [HideInInspector]
    public bool hasLight = false;

    [HideInInspector]
    public List<string> teaIngredients = new List<string>();

    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    private void Start()
    {
        InitializeIcons();
    }

    public void InitializeIcons()
    {
        teaIngredients.Clear();
        hasLight = false;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        ingredientIcons.Clear();
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
                string flowerType = child.name.Replace("IconTeapot", "");
                ingredientIcons[flowerType] = child.gameObject;
                child.gameObject.SetActive(false);
                Debug.Log($"🌸 Registered icon: {flowerType}");
            }
        }

        if (brewIcon == null)
        {
            Debug.LogWarning("⚠️ TeacupBrewIcon not found in teapot prefab!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
        }

        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null && holder.HasFlower())
        {
            AddIngredient(holder.currentFlowerType);
            ShowIngredientIcon(holder.currentFlowerType);
            holder.DropFlower();
        }
    }

    public bool HasAnyIngredients()
    {
        return ingredients != null && ingredients.Count > 0;
    }


    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (teapotAnimator != null)
            teapotAnimator.SetTrigger("Pulse");

        if (brewIcon != null)
            brewIcon.SetActive(true);

        Debug.Log("✨ Teapot charged with light!");
    }

    public void AddIngredient(string flowerType)
    {
        if (!teaIngredients.Contains(flowerType))
        {
            teaIngredients.Add(flowerType);
            Debug.Log("🫖 Added flower: " + flowerType);
        }
    }

    public void ShowIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out GameObject icon))
        {
            icon.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"⚠️ No icon registered for flower: {flowerType}");
        }
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Cannot brew without light.");
            return null;
        }

        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.SetIngredients(teaIngredients, hasLight);
        }

        ResetTeapot();

        return teacup;
    }

    private void ResetTeapot()
    {
        hasLight = false;
        teaIngredients.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewIcon != null)
            brewIcon.SetActive(false);

        foreach (var icon in ingredientIcons.Values)
        {
            icon.SetActive(false);
        }
    }
}
