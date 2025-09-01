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

    private void OnTriggerEnter2D(Collider2D other)
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

        Debug.Log("✨ Teapot is charged with light!");
    }

    private void Update()
    {
        if (hasLight && teaIngredients.Count > 0 && Input.GetKeyDown(brewKey))
        {
            BrewTea();
        }
    }

    public void AddIngredient(string flowerType)
    {
        teaIngredients.Add(flowerType);
        Debug.Log("🫖 Added flower to teapot: " + flowerType);
    }

    public void CreateIngredientIcon(string flowerType)
    {
        GameObject icon = Instantiate(iconPrefab, iconDisplayParent);
        // Optional: Set icon appearance based on flowerType
    }

    public void BrewTea()
    {
        if (teaIngredients.Count > 0)
        {
            GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
            TeaEffectManager effectManager = teacup.GetComponent<TeaEffectManager>();
            if (effectManager != null)
            {
                effectManager.SetIngredients(teaIngredients);
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

            gameObject.SetActive(false); // Disable teapot
        }
    }
}