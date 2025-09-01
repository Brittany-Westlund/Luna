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

    // State for detecting the player
    private bool playerIsNearby = false;
    public bool PlayerIsNearby => playerIsNearby;
    private FlowerHolder nearbyHolder;

    // Stored flowers and their types
    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    // Interaction cooldown
    private bool  canInteract        = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer      = 0f;

    // Light state
    public bool hasLight { get; private set; } = false;

    void Start()
    {
        // Cache all child icons
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "TeacupBrewIcon")
            {
                brewIcon = t.gameObject;
                brewIcon.SetActive(false);
            }
            else if (t.name.EndsWith("IconTeapot"))
            {
                string type = t.name.Replace("IconTeapot", "");
                ingredientIcons[type] = t.gameObject;
                t.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (!playerIsNearby || nearbyHolder == null)
            return;

        if (canInteract && Input.GetKeyDown(KeyCode.X))
        {
            if (nearbyHolder.HasFlower)
                AddFlowerToTeapot(nearbyHolder);
            else if (HasAnyIngredients())
                RetrieveLastFlower(nearbyHolder);

            canInteract = false;
        }

        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactionCooldown)
            {
                canInteract     = true;
                cooldownTimer   = 0f;
            }
        }
    }

    /// <summary>
    /// Hides the held flower, stacks it, lights its icon, and drops it from the holder.
    /// </summary>
    public void AddFlowerToTeapot(FlowerHolder holder)
    {
        if (!holder.HasFlower) return;

        GameObject flower = holder.GetHeldFlower();
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType
                      : "Unknown";
        if (type == "Unknown") return;

        // Hide and store
        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        // Show icon
        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(true);

        holder.DropFlower();
        Debug.Log($"🫖 Teapot: added {type}");
    }

    /// <summary>
    /// Reactivates and returns the last flower to the given holder.
    /// </summary>
    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int i = storedFlowers.Count - 1;
        if (i < 0) return;

        GameObject flower = storedFlowers[i];
        string    type   = teaIngredients[i];

        storedFlowers.RemoveAt(i);
        teaIngredients.RemoveAt(i);

        // Hide icon
        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        if (flower != null)
        {
            flower.SetActive(true);
            holder.PickUpFlower(flower);
            Debug.Log($"🌼 Teapot: retrieved {type}");
        }
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients.Count > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Charge from a light mote
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

        // Detect player’s FlowerHolder
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            nearbyHolder   = holder;
            playerIsNearby = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Keep the in-range state alive while staying in the collider
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            nearbyHolder   = holder;
            playerIsNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            nearbyHolder   = null;
            playerIsNearby = false;
        }
    }

    /// <summary>
    /// Public API for external scripts to charge the teapot.
    /// </summary>
    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect  != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon       != null) brewIcon.SetActive(true);
        Debug.Log("✨ Teapot: received light");
    }

    /// <summary>
    /// Brews a teacup with current ingredients, then resets the teapot.
    /// </summary>
    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var effect))
            effect.SetIngredients(teaIngredients, hasLight);

        // Reset state
        hasLight            = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon       != null) brewIcon.SetActive(false);
        foreach (var icon in ingredientIcons.Values)
            icon.SetActive(false);

        return cup;
    }
}
