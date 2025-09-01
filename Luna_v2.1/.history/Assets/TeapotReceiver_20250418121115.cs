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

    private List<string> teaIngredients     = new List<string>();
    private List<GameObject> storedFlowers = new List<GameObject>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    private bool  canInteract        = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer      = 0f;

    public bool hasLight { get; private set; } = false;

    void Start()
    {
        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "TeacupBrewIcon")
            {
                brewIcon = child.gameObject;
                brewIcon.SetActive(false);
            }
            else if (child.name.EndsWith("IconTeapot"))
            {
                string key = child.name.Replace("IconTeapot", "");
                ingredientIcons[key] = child.gameObject;
                child.gameObject.SetActive(false);
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

    public void AddFlowerToTeapot(FlowerHolder holder)
    {
        if (!holder.HasFlower) return;
        var flower = holder.GetHeldFlower();
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
                      ? pickup.flowerType : "Unknown";
        if (type == "Unknown") return;

        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);
        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(true);

        holder.DropFlower();
        Debug.Log($"🫖 Teapot: added {type}");
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int i = storedFlowers.Count - 1;
        if (i < 0) return;

        var flower = storedFlowers[i];
        var type   = teaIngredients[i];

        storedFlowers.RemoveAt(i);
        teaIngredients.RemoveAt(i);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        if (flower != null)
        {
            // **REACTIVATE** before handing back
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
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
            return;
        }

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

    public void ReceiveLight()
    {
        hasLight = true;
        sparkleEffect?.SetActive(true);
        teapotAnimator?.SetTrigger("Pulse");
        brewIcon?.SetActive(true);
        Debug.Log("✨ Teapot: received light");
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;
        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var eff))
            eff.SetIngredients(teaIngredients, hasLight);

        // reset
        hasLight            = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        sparkleEffect?.SetActive(false);
        brewIcon?.SetActive(false);
        foreach (var icon in ingredientIcons.Values)
            icon.SetActive(false);

        return cup;
    }

    // Keep the playerIsNearby/nearbyHolder alive every frame you stay in the pot
    void OnTriggerStay2D(Collider2D other)
    {
        var holder = other.GetComponentInParent<FlowerHolder>();
        if (holder != null)
        {
            nearbyHolder   = holder;
            playerIsNearby = true;
        }
    }


}


