using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    // Exposed so FlowerInteraction can know when to hand off X to the teapot
    private bool playerIsNearby = false;
    public bool PlayerIsNearby => playerIsNearby;

    private FlowerHolder nearbyHolder;

    // Stacked ingredients + visuals
    private List<string> teaIngredients = new();
    private List<GameObject> storedFlowerObjects = new();
    private Dictionary<string, GameObject> ingredientIcons = new();
    private GameObject brewIcon;

    // Simple cooldown so you don’t accidentally double‑tap
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
        teaIngredients.Clear();
        storedFlowerObjects.Clear();
        ingredientIcons.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        // Find all child GameObjects named “IconTeapot” to map them
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
        // Only run if we’re in range of the teapot
        if (!playerIsNearby || nearbyHolder == null)
            return;

        // On X press, either add or retrieve
        if (canInteract && Input.GetKeyDown(KeyCode.X))
        {
            if (nearbyHolder.HasFlower)
                AddFlowerToTeapot();
            else if (storedFlowerObjects.Count > 0)
                RetrieveFlowerFromTeapot();

            canInteract = false;
        }

        // Simple cooldown timer
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
        // Pull the actual flower object
        GameObject flower = nearbyHolder.GetHeldFlower();
        if (flower == null) 
        {
            Debug.LogWarning("🫖 Tried to add to teapot, but heldFlower was null.");
            return;
        }

        // Read its type directly from FlowerPickup
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType
                      : "Unknown";

        if (type == "Unknown")
        {
            Debug.LogWarning("🫖 Flower has no valid FlowerPickup.flowerType set!");
            return;
        }

        // Hide the flower, stack it, show icon
        flower.SetActive(false);
        storedFlowerObjects.Add(flower);
        teaIngredients.Add(type);
        ShowIngredientIcon(type);

        // Let the holder drop it
        nearbyHolder.DropFlower();
        Debug.Log($"🫖 Added {type} to teapot.");
    }

    private void RetrieveFlowerFromTeapot()
    {
        // Take the last‑added flower
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

        // Reactivate & hand it back to the holder
        flower.SetActive(true);
        nearbyHolder.PickUpFlower(flower);
        Debug.Log($"🌼 Retrieved {type} from teapot.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Light mote charging
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        // Player entering range?
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            nearbyHolder = holder;
            playerIsNearby = true;
            Debug.Log("🫖 Player in teapot range");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            nearbyHolder = null;
            playerIsNearby = false;
            Debug.Log("🫖 Player left teapot range");
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

    public bool HasAnyIngredients() => teaIngredients.Count > 0;

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
