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
    public GameObject brewIcon;

    [Header("Flower Icons")]
    public List<FlowerIconEntry> flowerIconEntries;

    private Dictionary<string, GameObject> flowerIconMap;

    [System.Serializable]
    public class FlowerIconEntry
    {
        public string flowerType;
        public GameObject iconPrefab;
    }

    private void Start()
    {
        flowerIconMap = new Dictionary<string, GameObject>();
        foreach (var entry in flowerIconEntries)
        {
            if (!flowerIconMap.ContainsKey(entry.flowerType))
            {
                flowerIconMap.Add(entry.flowerType, entry.iconPrefab);
            }
        }

        if (brewIcon != null)
        {
            brewIcon.SetActive(false);
        }
    }

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
        UpdateBrewIconVisibility();
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
        GameObject prefab = iconPrefab;

        if (flowerIconMap.TryGetValue(flowerType, out GameObject specificPrefab))
        {
            prefab = specificPrefab;
        }

        if (prefab != null && iconDisplayParent != null)
        {
            int index = iconDisplayParent.childCount;
            Vector3 offset = new Vector3(0.5f * index, 0, 0);
            Instantiate(prefab, iconDisplayParent.position + offset, Quaternion.identity, iconDisplayParent);
        }

        UpdateBrewIconVisibility();
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

            if (brewIcon != null)
            {
                brewIcon.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }

    private void UpdateBrewIconVisibility()
    {
        if (brewIcon == null) return;

        bool shouldShow = hasLight && teaIngredients.Count > 0;
        brewIcon.SetActive(shouldShow);
    }
}
