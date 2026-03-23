using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    private readonly List<GameObject> storedFlowers = new List<GameObject>();
    private readonly List<string> teaIngredients = new List<string>();
    private readonly Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();

    private GameObject brewIcon;

    public bool hasLight { get; set; }

    private void Awake()
    {
        if (teacupPrefab == null)
        {
            teacupPrefab = Resources.Load<GameObject>("Teacup");
            if (teacupPrefab == null)
                Debug.LogError("[TeapotReceiver] Teacup prefab not found in Resources/Teacup!");
        }

        if (teacupSpawnPoint == null)
        {
            Transform found = transform.Find("TeacupSpawnPoint");
            teacupSpawnPoint = found != null ? found : transform;
        }

        if (sparkleEffect == null)
            sparkleEffect = transform.Find("SparkleEffect")?.gameObject;
    }

    private void Start()
    {
        var lightReceiver = GetComponent<TeapotLightReceiver>();
        hasLight = lightReceiver != null ? lightReceiver.HasLight : false;

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
        if (holder == null || !holder.HasFlower)
            return;

        GameObject flower = holder.GetHeldFlower();
        if (flower == null)
            return;

        FlowerPickup pickup = flower.GetComponent<FlowerPickup>();
        string type = (pickup != null && !string.IsNullOrEmpty(pickup.flowerType))
            ? pickup.flowerType
            : "Unknown";

        if (type == "Unknown")
            return;

        HideHintIcons(flower);

        flower.SetActive(false);
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        Debug.Log($"[TeapotReceiver] Added flower '{flower.name}' as ingredient '{type}'. Count now: {storedFlowers.Count}");

        var lightRec = GetComponent<TeapotLightReceiver>();
        if (lightRec != null)
            lightRec.AddIngredient(flower);

        if (ingredientIcons.TryGetValue(type, out GameObject icon) && icon != null)
            icon.SetActive(true);

        holder.DropFlower();
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        if (holder == null)
            return;

        int idx = storedFlowers.Count - 1;
        if (idx < 0)
            return;

        GameObject flower = storedFlowers[idx];
        string type = teaIngredients[idx];

        storedFlowers.RemoveAt(idx);
        teaIngredients.RemoveAt(idx);

        if (ingredientIcons.TryGetValue(type, out GameObject icon) && icon != null)
            icon.SetActive(false);

        if (flower != null)
        {
            ShowHintIcons(flower);
            flower.SetActive(true);
            holder.PickUpFlower(flower);
        }
    }

    public bool HasAnyIngredients() => teaIngredients.Count > 0;

    public int GetIngredientCount()
    {
        return teaIngredients.Count;
    }

    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (teapotAnimator != null)
            teapotAnimator.SetTrigger("Pulse");

        if (brewIcon != null)
            brewIcon.SetActive(true);
    }

    public GameObject BrewTea()
    {
        Debug.Log($"[TeapotReceiver] BrewTea called. hasLight={hasLight}, ingredientCount={storedFlowers.Count}, teacupPrefab={(teacupPrefab != null ? teacupPrefab.name : "NULL")}, spawnPoint={(teacupSpawnPoint != null ? teacupSpawnPoint.name : "NULL")}");

        var waterReceiver = GetComponent<TeapotWaterReceiver>();
        if (waterReceiver == null)
            waterReceiver = GetComponentInChildren<TeapotWaterReceiver>(true);

        if (waterReceiver != null && !waterReceiver.HasWater())
        {
            Debug.LogWarning("[TeapotReceiver] Brew blocked: teapot has no water.");
            return null;
        }

        if (!hasLight)
        {
            Debug.LogWarning("[TeapotReceiver] Brew blocked: teapot has no light.");
            return null;
        }

        if (teacupPrefab == null)
        {
            Debug.LogError("[TeapotReceiver] Brew failed: teacupPrefab is null.");
            return null;
        }

        if (teacupSpawnPoint == null)
        {
            Debug.LogError("[TeapotReceiver] Brew failed: teacupSpawnPoint is null.");
            return null;
        }

        foreach (GameObject flower in storedFlowers)
        {
            if (flower == null)
                continue;

            var sprout = flower.GetComponent<SproutAndLightManager>();
            if (sprout != null)
                sprout.BrewFlower();

            Destroy(flower);
        }

        GameObject cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        if (cup == null)
        {
            Debug.LogError("[TeapotReceiver] Brew failed: Instantiate returned null.");
            return null;
        }

        var sfx = cup.GetComponent<TeacupBrewSFX>();
        if (sfx != null)
            sfx.PlayBrewSound();
        else
            Debug.LogWarning("[TeapotReceiver] TeacupBrewSFX not found on new teacup.");

        var eff = cup.GetComponent<TeaEffectManager>();
        if (eff != null)
            eff.SetIngredients(teaIngredients, hasLight);

        if (waterReceiver != null)
        {
            bool consumed = waterReceiver.TryConsumeWaterForBrewing();
            Debug.Log($"[TeapotReceiver] Water consumed for brewing: {consumed}");
        }

        var lightReceiver = GetComponent<TeapotLightReceiver>();
        hasLight = lightReceiver != null ? !lightReceiver.requireLight : false;

        teaIngredients.Clear();
        storedFlowers.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewIcon != null)
            brewIcon.SetActive(false);

        foreach (var kv in ingredientIcons)
        {
            if (kv.Value != null)
                kv.Value.SetActive(false);
        }

        return cup;
    }

    private void HideHintIcons(GameObject flower)
    {
        foreach (SpriteRenderer sr in flower.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.gameObject.name.Contains("SporeHintIcon") || sr.gameObject.name.Contains("LightMoteIcon"))
                sr.enabled = false;
        }
    }

    private void ShowHintIcons(GameObject flower)
    {
        foreach (SpriteRenderer sr in flower.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.gameObject.name.Contains("SporeHintIcon") || sr.gameObject.name.Contains("LightMoteIcon"))
                sr.enabled = true;
        }
    }
}