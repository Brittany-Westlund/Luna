// TeapotReceiver.cs
using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    // Exposed for FlowerInteraction to know when to hand off X
    private bool playerIsNearby = false;
    public bool PlayerIsNearby => playerIsNearby;

    private FlowerHolder nearbyHolder;

    private List<string> teaIngredients = new List<string>();
    private List<GameObject> storedFlowerObjects = new List<GameObject>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    private bool canInteract = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;

    public bool hasLight { get; private set; } = false;

    void Start()
    {
        InitializeIcons();
    }

    private void InitializeIcons()
    {
        ingredientIcons.Clear();
        teaIngredients.Clear();
        storedFlowerObjects.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "TeacupBrewIcon")
            {
                brewIcon = child.gameObject;
                brewIcon.SetActive(false);
            }
            else if (child.name.EndsWith("IconTeapot"))
            {
                string flowerType = child.name.Replace("IconTeapot", "");
                ingredientIcons[flowerType] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }

        if (brewIcon == null)
            Debug.LogWarning("⚠️ TeacupBrewIcon not found in teapot children!");
    }

    void Update()
    {
        if (!playerIsNearby || nearbyHolder == null) return;

        if (canInteract && Input.GetKeyDown(KeyCode.X))
        {
            if (nearbyHolder.HasFlower)
                AddFlowerToTeapot();
            else if (storedFlowerObjects.Count > 0)
                RetrieveFlowerFromTeapot();

            canInteract = false;
        }

        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactionCooldown)
            {
                canInteract = true;
                cooldownTimer = 0f;
            }
        }
    }

    private void AddFlowerToTeapot()
    {
        GameObject flower = nearbyHolder.GetHeldFlower();
        if (flower == null)
        {
            Debug.LogWarning("🫖 Tried to add to teapot, but heldFlower was null.");
            return;
        }

        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType
                      : "Unknown";

        if (type == "Unknown")
        {
            Debug.LogWarning("🫖 Flower has no valid FlowerPickup.flowerType set!");
            return;
        }

        flower.SetActive(false);
        storedFlowerObjects.Add(flower);
        teaIngredients.Add(type);
        ShowIngredientIcon(type);

        nearbyHolder.DropFlower();
        Debug.Log($"🫖 Added {type} to teapot.");
    }

    private void RetrieveFlowerFromTeapot()
    {
        int last = storedFlowerObjects.Count - 1;
        GameObject flower = storedFlowerObjects[last];
        string type = teaIngredients[last];

        storedFlowerObjects.RemoveAt(last);
        teaIngredients.RemoveAt(last);
        HideIngredientIcon(type);

        if (flower == null)
        {
            Debug.LogWarning("🌼 Tried to retrieve, but flower was null.");
            return;
        }

        flower.SetActive(true);
        nearbyHolder.PickUpFlower(flower);
        Debug.Log($"🌼 Retrieved {type} from teapot.");
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
        if (holder != null)
        {
            nearbyHolder = holder;
            playerIsNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            nearbyHolder = null;
            playerIsNearby = false;
        }
    }

    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon     != null) brewIcon.SetActive(true);
        Debug.Log("✨ Teapot charged with light!");
    }

    private void ShowIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out var icon))
            icon.SetActive(true);
        else
            Debug.LogWarning($"⚠️ No icon set for {flowerType}");
    }

    private void HideIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out var icon))
            icon.SetActive(false);
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Cannot brew without light.");
            return null;
        }
        var teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (teacup.TryGetComponent<TeaEffectManager>(out var effect))
            effect.SetIngredients(teaIngredients, hasLight);
        ResetTeapot();
        return teacup;
    }

    private void ResetTeapot()
    {
        hasLight = false;
        teaIngredients.Clear();
        storedFlowerObjects.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon     != null) brewIcon.SetActive(false);
        foreach (var icon in ingredientIcons.Values)
            icon.SetActive(false);
    }
}
