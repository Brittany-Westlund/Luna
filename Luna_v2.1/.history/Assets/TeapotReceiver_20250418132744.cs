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

    // Internal storage
    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    // Charged state
    public bool hasLight { get; private set; }

    void Start()
    {
        hasLight = false;
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
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

    public void AddFlowerToTeapot(FlowerHolder holder)
    {
        if (!holder.HasFlower) return;

        GameObject flower = holder.GetHeldFlower();
        FlowerPickup meta = flower.GetComponent<FlowerPickup>();
        string type = (meta != null && meta.flowerType != null && meta.flowerType != "")
                      ? meta.flowerType
                      : "Unknown";
        if (type == "Unknown") return;

        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        GameObject icon;
        if (ingredientIcons.TryGetValue(type, out icon))
            icon.SetActive(true);

        holder.DropFlower();
        Debug.Log("🫖 Teapot: added " + type);
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int idx = storedFlowers.Count - 1;
        if (idx < 0) return;

        GameObject flower = storedFlowers[idx];
        string    type   = teaIngredients[idx];

        storedFlowers.RemoveAt(idx);
        teaIngredients.RemoveAt(idx);

        GameObject icon;
        if (ingredientIcons.TryGetValue(type, out icon))
            icon.SetActive(false);

        if (flower != null)
        {
            flower.SetActive(true);
            holder.PickUpFlower(flower);
            Debug.Log("🌼 Teapot: retrieved " + type);
        }
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients.Count > 0;
    }

    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon != null)       brewIcon.SetActive(true);
        Debug.Log("✨ Teapot: received light");
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        GameObject cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager eff = cup.GetComponent<TeaEffectManager>();
        if (eff != null)
            eff.SetIngredients(teaIngredients, hasLight);

        // Reset
        hasLight           = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon      != null) brewIcon.SetActive(false);
        foreach (KeyValuePair<string, GameObject> kv in ingredientIcons)
            kv.Value.SetActive(false);

        return cup;
    }
}
