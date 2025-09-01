using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

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

        var flower = holder.GetHeldFlower();
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && pickup.flowerType != "") 
                      ? pickup.flowerType 
                      : "Unknown";
        if (type == "Unknown") return;

        // ─── HIDE ANY HINT ICONS ───────────────────────────
        HideHintIcons(flower);

        // ─── STORE THE FLOWER ──────────────────────────────
        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(true);

        holder.DropFlower();
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int idx = storedFlowers.Count - 1;
        if (idx < 0) return;

        var flower = storedFlowers[idx];
        var type   = teaIngredients[idx];

        storedFlowers.RemoveAt(idx);
        teaIngredients.RemoveAt(idx);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        if (flower != null)
        {
            // ─── SHOW THE HINT ICONS AGAIN ───────────────────
            ShowHintIcons(flower);

            flower.SetActive(true);
            holder.PickUpFlower(flower);
        }
    }

    public bool HasAnyIngredients() => teaIngredients.Count > 0;

    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon != null)       brewIcon.SetActive(true);
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        var eff = cup.GetComponent<TeaEffectManager>();
        if (eff != null)
            eff.SetIngredients(teaIngredients, hasLight);

        hasLight           = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon      != null) brewIcon.SetActive(false);
        foreach (var kv in ingredientIcons)
            kv.Value.SetActive(false);

        return cup;
    }

    // ─── HINT ICON HELPERS ───────────────────────────────

    void HideHintIcons(GameObject flower)
    {
        foreach (var sr in flower.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.gameObject.name.Contains("SporeHintIcon") 
             || sr.gameObject.name.Contains("LightMoteIcon"))
            {
                sr.enabled = false;
            }
        }
    }

    void ShowHintIcons(GameObject flower)
    {
        foreach (var sr in flower.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.gameObject.name.Contains("SporeHintIcon") 
             || sr.gameObject.name.Contains("LightMoteIcon"))
            {
                sr.enabled = true;
            }
        }
    }
}


/* using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    private List<GameObject> storedFlowers   = new List<GameObject>();
    private List<string>     teaIngredients  = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

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

        var flower = holder.GetHeldFlower();
        var pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && pickup.flowerType != "") 
                      ? pickup.flowerType 
                      : "Unknown";
        if (type == "Unknown") return;

        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        GameObject icon;
        if (ingredientIcons.TryGetValue(type, out icon))
            icon.SetActive(true);

        holder.DropFlower();
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        int idx = storedFlowers.Count - 1;
        if (idx < 0) return;

        var flower = storedFlowers[idx];
        var type   = teaIngredients[idx];

        storedFlowers.RemoveAt(idx);
        teaIngredients.RemoveAt(idx);

        GameObject icon;
        if (ingredientIcons.TryGetValue(type, out icon))
            icon.SetActive(false);

        if (flower != null)
        {
            flower.SetActive(true);
            holder.PickUpFlower(flower);
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
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        var eff = cup.GetComponent<TeaEffectManager>();
        if (eff != null)
            eff.SetIngredients(teaIngredients, hasLight);

        hasLight           = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon      != null) brewIcon.SetActive(false);
        foreach (var kv in ingredientIcons)
            kv.Value.SetActive(false);

        return cup;
    }
}
*/