using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("Brewing Components")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    [Header("Visuals")]
    public GameObject sparkleEffect;
    public Animator teapotAnimator;
    private GameObject brewIcon; // TeacupBrewIcon, found in Start()

    [Header("State Tracking")]
    public bool hasLight = false;
    public List<string> teaIngredients = new List<string>();

    private void Start()
    {
        // Find brew icon even if it's inactive
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "TeacupBrewIcon")
            {
                brewIcon = child.gameObject;
                brewIcon.SetActive(false);
                Debug.Log("✅ Found TeacupBrewIcon.");
                break;
            }
        }

        if (brewIcon == null)
        {
            Debug.LogWarning("⚠️ TeacupBrewIcon not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Light mote interaction
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        // Flower delivery
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null && holder.HasFlower())
        {
            AddIngredient(holder.currentFlowerType);
            holder.DropFlower(); // Clear it from player
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
        Debug.Log("🌸 Added ingredient: " + flowerType);
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Can't brew without light.");
            return null;
        }

        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);

        // Pass tea properties to the cup
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.SetIngredients(teaIngredients, brewedWithLight: true);
        }

        // Reset state
        teaIngredients.Clear();
        hasLight = false;

        if (brewIcon != null) brewIcon.SetActive(false);
        if (sparkleEffect != null) sparkleEffect.SetActive(false);

        Debug.Log("🫖 Tea brewed successfully.");
        return teacup;
    }
}
