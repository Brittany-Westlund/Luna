using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    [HideInInspector] public bool hasLight = false;

    private bool playerIsNearby = false;
    private FlowerHolder nearbyHolder;

    private List<string> teaIngredients = new List<string>();
    private List<GameObject> storedFlowerObjects = new List<GameObject>();

    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    void Start()
    {
        InitializeIcons();
    }

    public void InitializeIcons()
    {
        teaIngredients.Clear();
        hasLight = false;
        storedFlowerObjects.Clear();

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

    private void Update()
    {
        if (!playerIsNearby || nearbyHolder == null) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (nearbyHolder.HasFlower())
            {
                // 🌿 Add flower to teapot
                GameObject flower = nearbyHolder.GetHeldFlower();
                string type = nearbyHolder.currentFlowerType;

                AddIngredient(type);
                ShowIngredientIcon(type);

                flower.SetActive(false); // Preserve but hide
                storedFlowerObjects.Add(flower);
                teaIngredients.Add(type);

                nearbyHolder.DropFlower();
                Debug.Log("🫖 Flower added to teapot: " + type);
            }
            else if (storedFlowerObjects.Count > 0)
            {
                // 🌸 Retrieve last-added flower
                GameObject flower = storedFlowerObjects[^1]; // last item
                string type = teaIngredients[^1];

                storedFlowerObjects.RemoveAt(storedFlowerObjects.Count - 1);
                teaIngredients.RemoveAt(teaIngredients.Count - 1);

                HideIngredientIcon(type);

                flower.SetActive(true);
                flower.transform.position = nearbyHolder.holdPoint.position;
                nearbyHolder.PickUpFlowerInstance(flower);

                Debug.Log("🔁 Flower retrieved from teapot: " + type);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            playerIsNearby = true;
            nearbyHolder = holder;
        }

        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            playerIsNearby = false;
            nearbyHolder = null;
        }
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients != null && teaIngredients.Count > 0;
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

    public void HideIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out GameObject icon))
        {
            icon.SetActive(false);
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
        storedFlowerObjects.Clear();

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
