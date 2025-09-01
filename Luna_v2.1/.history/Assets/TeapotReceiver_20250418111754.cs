using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    // For FlowerInteraction to know when to hand off X
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

    void InitializeIcons()
    {
        teaIngredients.Clear();
        storedFlowerObjects.Clear();
        ingredientIcons.Clear();

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

    void AddFlowerToTeapot()
    {
        var flower = nearbyHolder.GetHeldFlower();
        if (flower == null) return;

        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType
                      : "Unknown";
        if (type == "Unknown") return;

        flower.SetActive(false);
        storedFlowerObjects.Add(flower);
        teaIngredients.Add(type);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(true);

        nearbyHolder.DropFlower();
    }

    void RetrieveFlowerFromTeapot()
    {
        int last = storedFlowerObjects.Count - 1;
        var flower = storedFlowerObjects[last];
        string type = teaIngredients[last];

        storedFlowerObjects.RemoveAt(last);
        teaIngredients.RemoveAt(last);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        if (flower != null)
            nearbyHolder.PickUpFlower(flower);
    }

    /// <summary>
    /// Returns true if there’s at least one ingredient stacked.
    /// </summary>
    public bool HasAnyIngredients() => teaIngredients.Count > 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Light mote charging
        if (other.CompareTag("LightMote"))
        {
            // expose this so other scripts can call as well
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        // Detect player entering teapot range
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            nearbyHolder = holder;
            playerIsNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            nearbyHolder = null;
            playerIsNearby = false;
        }
    }

    /// <summary>
    /// Public method so external scripts (e.g. LunariaWandAttractor) can charge the teapot with light.
    /// </summary>
    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (teapotAnimator != null)
            teapotAnimator.SetTrigger("Pulse");

        if (brewIcon != null)
            brewIcon.SetActive(true);

        Debug.Log("✨ TeapotReceiver: ReceiveLight() called, teapot is now charged.");
    }

    /// <summary>
    /// Instantiates a teacup with the current ingredients and light state,
    /// then resets the teapot for next brew.
    /// </summary>
    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Cannot brew without light.");
            return null;
        }

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var effect))
            effect.SetIngredients(teaIngredients, hasLight);

        // Reset
        hasLight = false;
        teaIngredients.Clear();
        storedFlowerObjects.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon     != null) brewIcon.SetActive(false);
        foreach (var icon in ingredientIcons.Values)
            icon.SetActive(false);

        return cup;
    }
}
