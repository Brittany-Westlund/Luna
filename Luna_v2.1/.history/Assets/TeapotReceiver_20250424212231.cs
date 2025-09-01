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

    public bool hasLight { get; set; }
    void Awake()
    {
        if (teacupPrefab == null)
        {
            teacupPrefab = Resources.Load<GameObject>("Teacup");
            if (teacupPrefab == null)
                Debug.LogError("Teacup prefab not found in Resources/Teacup!");
        }

        // Find your spawn point (in children) if not assigned:
        if (teacupSpawnPoint == null)
        {
            var found = transform.Find("TeacupSpawnPoint");
            if (found != null)
                teacupSpawnPoint = found;
            else
                teacupSpawnPoint = this.transform; // fallback to self
        }

        // ... assign any other effects/fields via transform.Find, e.g.:
        if (sparkleEffect == null)
            sparkleEffect = transform.Find("SparkleEffect")?.gameObject;
    }
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
        Debug.Log($"[TeapotReceiver] Adding flower: {flower.name} to storedFlowers. Total now: {storedFlowers.Count + 1}");
        storedFlowers.Add(flower);
        teaIngredients.Add(type);

        // ─── ALSO TELL THE LIGHT‐RECEIVER ABOUT THIS INGREDIENT ───
        var lightRec = GetComponent<TeapotLightReceiver>();
        if (lightRec != null)
            lightRec.AddIngredient(flower);

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

    public int GetIngredientCount()
    {
        return teaIngredients.Count;
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
        Debug.Log("[TeapotReceiver] BrewTea CALLED!");
        Debug.Log("[TeapotReceiver] BrewTea CALLED!");
    Debug.Log("hasLight: " + hasLight);
    Debug.Log("storedFlowers: " + storedFlowers.Count);
    Debug.Log("teacupPrefab: " + (teacupPrefab != null ? teacupPrefab.name : "NULL"));
    Debug.Log("teacupSpawnPoint: " + (teacupSpawnPoint != null ? teacupSpawnPoint.name : "NULL"));
    Debug.Log("ScoreManager.Instance: " + (ScoreManager.Instance != null ? "set" : "NULL"));

   

        if (!hasLight) return null;
        Debug.Log("[TeapotReceiver] BrewTea() called!");
        // Remove the light point for every flower that was lit
        Debug.Log($"[TeapotReceiver] BrewTea called. storedFlowers count: {storedFlowers.Count}");
        foreach (var flower in storedFlowers)
        {
            if (flower != null)
            {
                Debug.Log($"[TeapotReceiver] Processing flower: {flower.name}");

                var sprout = flower.GetComponent<SproutAndLightManager>();
                if (sprout != null)
                {
                    Debug.Log($"[TeapotReceiver] Found SproutAndLightManager on: {flower.name}");
                    sprout.BrewFlower();
                }
                else
                {
                    Debug.Log($"[TeapotReceiver] NO SproutAndLightManager on: {flower.name}");
                }

                Destroy(flower); // <-- Destroy flower object after brewing
            }
            else
            {
                Debug.Log("[TeapotReceiver] Null flower in storedFlowers!");
            }
        }

        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        
        // PLAY BREW SOUND ON THE TEACUP
        var sfx = cup.GetComponent<TeacupBrewSFX>();
        Debug.Log($"[BREW] sfx is null? {sfx == null}");
        if (sfx != null)
        {
            Debug.Log("Calling PlayBrewSound() on Teacup (Receiver)");
            sfx.PlayBrewSound();
        }
        else
        {
            Debug.LogWarning("TeacupBrewSFX not found on new Teacup! (Receiver)");
        }
        
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
