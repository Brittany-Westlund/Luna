using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    // Stored state
    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    // Charged state
    public bool hasLight { get; private set; }

    void Start()
    {
        hasLight = false;
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

    /// <summary>
    /// Call when Luna presses X holding a flower.
    /// </summary>
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

    /// <summary>
    /// Call when Luna presses X empty‑handed.
    /// </summary>
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
            flower.SetActive(true);
            holder.PickUpFlower(flower);
            Debug.Log($"🌼 Teapot: retrieved {type}");
        }
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients.Count > 0;
    }

    /// <summary>
    /// Triggered by wand/light to charge.
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
    /// Brew a teacup and reset.
    /// </summary>
    public GameObject BrewTea()
    {
        if (!hasLight) return null;
        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup.TryGetComponent<TeaEffectManager>(out var eff))
            eff.SetIngredients(teaIngredients, hasLight);

        // Reset
        hasLight           = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        sparkleEffect?.SetActive(false);
        brewIcon?.SetActive(false);
        foreach (var ic in ingredientIcons.Values)
            ic.SetActive(false);

        return cup;
    }
}
