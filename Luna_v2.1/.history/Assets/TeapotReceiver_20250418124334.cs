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

    // In‑range tracking
    private bool playerIsNearby = false;
    public bool PlayerIsNearby => playerIsNearby;
    private FlowerHolder nearbyHolder;

    // Stored data
    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    // Cooldown
    private bool  canInteract         = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer       = 0f;

    // Lighted state
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
                canInteract   = true;
                cooldownTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Hide, store, and icon‑light a held flower.
    /// </summary>
    public void AddFlowerToTeapot(FlowerHolder holder)
    {
        if (!holder.HasFlower) return;

        var flower = holder.GetHeldFlower();
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType
                      : "Unknown";
        if (type == "Unknown") return;

        // Hide and store
        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        // Show its icon
        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(true);

        holder.DropFlower();
        Debug.Log($"🫖 Teapot: added {type}");
    }

    /// <summary>
    /// Reactivate and return the last‑added flower.
    /// </summary>
    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int i = storedFlowers.Count - 1;
        if (i < 0) return;

        var flower = storedFlowers[i];
        var type   = teaIngredients[i];

        storedFlowers.RemoveAt(i);
        teaIngredients.RemoveAt(i);

        // Hide its icon
        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        // Reactivate and hand back
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
        // Charge from light
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
        // Remain “in range” if any player collider (body or held flower) overlaps
        if (other.CompareTag("Player") || other.GetComponentInParent<FlowerHolder>() != null)
        {
            nearbyHolder   = other.GetComponentInParent<FlowerHolder>();
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
    /// Exposed for wand/light scripts.
    /// </summary>
    public void ReceiveLight()
    {
        hasLight = true;
        sparkleEffect?.SetActive(true);
        teapotAnimator?.SetTrigger("Pulse");
        brewIcon?.SetActive(true);
        Debug.Log("✨ Teapot: received light");
    }

    /// <summary>
    /// Brew and reset.
    /// </summary>
    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var eff))
            eff.SetIngredients(teaIngredients, hasLight);

        // Reset state
        hasLight            = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        sparkleEffect?.SetActive(false);
        brewIcon?.SetActive(false);
        foreach (var ic in ingredientIcons.Values)
            ic.SetActive(false);

        return cup;
    }
}
