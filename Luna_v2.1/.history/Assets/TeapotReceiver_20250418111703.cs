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
        ingredientIcons[type].SetActive(true);

        nearbyHolder.DropFlower();
    }

    void RetrieveFlowerFromTeapot()
    {
        int last = storedFlowerObjects.Count - 1;
        var flower = storedFlowerObjects[last];
        string type = teaIngredients[last];

        storedFlowerObjects.RemoveAt(last);
        teaIngredients.RemoveAt(last);
        ingredientIcons[type].SetActive(false);

        if (flower != null)
            nearbyHolder.PickUpFlower(flower);
    }

    public bool HasAnyIngredients() => teaIngredients.Count > 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            hasLight = true;
            if (sparkleEffect != null) sparkleEffect.SetActive(true);
            if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
            if (brewIcon     != null) brewIcon.SetActive(true);
            Destroy(other.gameObject);
            return;
        }

        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
            nearbyHolder = holder;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
            nearbyHolder = null;
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var effect))
            effect.SetIngredients(teaIngredients, hasLight);

        // reset
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
