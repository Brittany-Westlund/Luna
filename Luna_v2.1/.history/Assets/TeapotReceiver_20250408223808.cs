using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    public List<string> teaIngredients = new List<string>();

    [Header("Icon Display")]
    public Transform iconDisplayParent;
    public GameObject brewIcon;

    [Header("Teacup Settings")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    [Header("Light & Sparkle")]
    public bool hasLight = false;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;
    public KeyCode brewKey = KeyCode.T;

    private void Start()
    {
        if (brewIcon != null)
        {
            brewIcon.SetActive(false);
        }
        else
        {
            Debug.LogWarning("🛑 brewIcon not assigned in TeapotReceiver!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
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
        string iconName = flowerType + "IconTeapot";
        Transform existingIcon = iconDisplayParent.Find(iconName);

        if (existingIcon != null)
        {
            existingIcon.gameObject.SetActive(true);
            Debug.Log($"✅ Activated icon: {iconName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No icon found with name: {iconName}");
        }
    }

    public void BrewTea()
    {
        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effectManager = teacup.GetComponent<TeaEffectManager>();
        if (effectManager != null)
        {
            effectManager.SetIngredients(teaIngredients, hasLight);
        }

        teaIngredients.Clear();

        foreach (Transform child in iconDisplayParent)
        {
            if (child.name != brewIcon.name)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (sparkleEffect != null)
        {
            sparkleEffect.SetActive(false);
        }

        if (brewIcon != null)
        {
            brewIcon.SetActive(false);
        }

        gameObject.SetActive(false); // Hide the teapot for now (optional)
    }

    private void UpdateBrewIcon()
    {
        if (brewIcon != null)
        {
            brewIcon.SetActive(hasLight);
        }
    }
}
