using System.Collections;
using UnityEngine;

public class WandForever : MonoBehaviour
{
    public GameObject wandChild;

    void Awake()
    {
        // 🔍 Find wandChild if not assigned
        if (wandChild == null)
        {
            var attractor = GetComponentInChildren<LunariaWandAttractor>(true);
            if (attractor != null)
                wandChild = attractor.gameObject;
        }

        // Start disabled; we’ll turn it on after save data loads
        if (wandChild != null)
            wandChild.SetActive(false);
    }

    void Start()
    {
        // ⏳ Wait one frame to let CollectibleState finish loading
        StartCoroutine(CheckAfterLoad());
    }

    IEnumerator CheckAfterLoad()
    {
        yield return null; // ensures CollectibleState.OnEnable() has finished

        bool hasCollectedWand = CollectibleManager.Instance != null &&
                                CollectibleManager.Instance.HasCollected("Wand01");

        if (hasCollectedWand)
        {
            UnlockWand();
            Debug.Log("🌕 Luna’s wand restored from collectibles.json");
        }
    }

    public void UnlockWand()
    {
        if (wandChild != null)
            wandChild.SetActive(true);

        if (CollectibleManager.Instance != null)
            CollectibleManager.Instance.MarkCollected("Wand01");

        Debug.Log("✨ Luna now permanently has the wand.");
    }
}
