using System.Collections;
using UnityEngine;

public class WandForever : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign Luna’s wand GameObject here (or leave empty to auto-find).")]
    public GameObject wandChild;

    [Header("Collectible ID")]
    [Tooltip("This must match the ID stored in your CollectibleState.")]
    public string wandID = "Wand01";

    private void Awake()
    {
        // 🔍 Auto-find wand child if not manually assigned
        if (wandChild == null)
        {
            var attractor = GetComponentInChildren<LunariaWandAttractor>(true);
            if (attractor != null)
                wandChild = attractor.gameObject;
        }

        // ✅ Always start disabled (prevents startup flash in build)
        if (wandChild != null)
            wandChild.SetActive(false);
    }

    private void Start()
    {
        // ⏳ Give CollectibleManager/JSON time to load
        StartCoroutine(CheckAfterLoad());
    }

    private IEnumerator CheckAfterLoad()
    {
        // wait a short bit longer than one frame for builds
        yield return new WaitForSeconds(0.2f);

        if (CollectibleManager.Instance == null)
        {
            Debug.LogWarning("[WandForever] CollectibleManager not found; wand stays hidden.");
            yield break;
        }

        bool hasWand = CollectibleManager.Instance.HasCollected(wandID);
        Debug.Log($"[WandForever] Checking wand '{wandID}' collected? {hasWand}");

        if (hasWand)
        {
            UnlockWand(false); // false = don't re-save to file
        }
        else
        {
            LockWand();
            Debug.Log("[WandForever] Wand not collected yet — remaining hidden.");
        }
    }

    /// <summary>
    /// Called when Luna first earns the wand (e.g. after dialogue or quest).
    /// </summary>
    /// <param name="saveToFile">If true, records ownership permanently.</param>
    public void UnlockWand(bool saveToFile = true)
    {
        if (wandChild != null)
            wandChild.SetActive(true);

        if (saveToFile && CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.MarkCollected(wandID);
            Debug.Log($"✨ Luna now permanently owns {wandID}.");
        }
    }

    /// <summary>
    /// Manually lock/remove the wand (useful for testing or story resets).
    /// </summary>
    public void LockWand()
    {
        if (wandChild != null)
            wandChild.SetActive(false);

        Debug.Log($"[WandForever] {wandID} locked/hidden.");
    }
}
