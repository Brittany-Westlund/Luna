
using UnityEngine;

public class FlowerDiagnostic : MonoBehaviour
{
    void Start()
    {
        var flowers = FindObjectsOfType<SproutAndLightManager>();
        Debug.Log($"[🌸 DIAGNOSTIC] Found {flowers.Length} flowers in the scene.");

        foreach (var flower in flowers)
        {
            var name = flower.gameObject.name;
            var hasPickup = flower.GetComponent<FlowerPickup>() != null;
            var hasCollider = flower.GetComponent<Collider2D>()?.enabled ?? false;
            var hasPivot = flower.transform.Find("pivot") != null;
            var hasLit = flower.transform.Find("LitFlowerB") != null;
            var scale = flower.transform.localScale;
            var isHeld = flower.isHeld;

            Debug.Log($"[🌼 {name}] Pickup:{hasPickup} | Collider:{hasCollider} | Pivot:{hasPivot} | Lit:{hasLit} | Scale:{scale} | Held:{isHeld}");
        }
    }
}
