// TeapotReceiver.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class TeapotReceiver : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject teacupPrefab;
    public Transform  teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator   teapotAnimator;

    private readonly List<GameObject> storedFlowers  = new List<GameObject>();
    private readonly List<string>     teaIngredients = new List<string>();
    private Dictionary<string, GameObject> ingredientIcons;
    private GameObject brewIcon;

    public bool hasLight { get; private set; }

    void Start()
    {
        hasLight = false;
        ingredientIcons = new Dictionary<string, GameObject>();

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "TeacupBrewIcon")
            {
                brewIcon = t.gameObject;
                brewIcon.SetActive(false);
            }
            else if (t.name.StartsWith("IconTeapot"))
            {
                var type = t.name.Replace("IconTeapot", "");
                ingredientIcons[type] = t.gameObject;
                t.gameObject.SetActive(false);
            }
        }
    }

    public void AddFlowerToTeapot(FlowerHolder holder)
    {
        if (!holder.HasFlower) return;
        var go = holder.GetHeldFlower();
        var pu = go.GetComponent<FlowerPickup>();
        if (pu == null || string.IsNullOrEmpty(pu.flowerType)) return;

        go.SetActive(false);
        storedFlowers.Add(go);
        teaIngredients.Add(pu.flowerType);

        if (ingredientIcons.TryGetValue(pu.flowerType, out var icon))
            icon.SetActive(true);

        holder.DropFlower();
        Debug.Log("🫖 Added " + pu.flowerType);
    }

    public void RetrieveLastFlower(FlowerHolder holder)
    {
        if (storedFlowers.Count == 0) return;
        int idx = storedFlowers.Count - 1;
        var go   = storedFlowers[idx];
        var type = teaIngredients[idx];

        storedFlowers.RemoveAt(idx);
        teaIngredients.RemoveAt(idx);

        if (ingredientIcons.TryGetValue(type, out var icon))
            icon.SetActive(false);

        go.SetActive(true);
        holder.PickUpFlower(go);
        Debug.Log("🌼 Retrieved " + type);
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients.Count > 0;
    }

    public void ReceiveLight()
    {
        if (teaIngredients.Count == 0) return;
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        if (brewIcon      != null) brewIcon.SetActive(true);
        Debug.Log("✨ Teapot lit");
    }

    public GameObject BrewTea()
    {
        if (!hasLight) return null;
        var cup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        var eff = cup.GetComponent<TeaEffectManager>();
        if (eff != null) eff.SetIngredients(teaIngredients, hasLight);

        // reset
        hasLight = false;
        teaIngredients.Clear();
        storedFlowers.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (brewIcon      != null) brewIcon.SetActive(false);
        foreach (var kv in ingredientIcons)
            kv.Value.SetActive(false);

        return cup;
    }
}
