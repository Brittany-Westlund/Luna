using UnityEngine;

/// <summary>
/// 🌙 Simple failsafe: press Q to toggle Luna’s wand on/off.
/// This never affects the collectibles save file — just visibility.
/// </summary>
public class WandToggleKey : MonoBehaviour
{
    [SerializeField] private WandForever wandForever; // optional explicit reference

    void Awake()
    {
        // Auto-find if not assigned
        if (wandForever == null)
            wandForever = GetComponent<WandForever>();
    }

    void Update()
    {
        if (wandForever == null) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            var wandChild = wandForever.wandChild;
            if (wandChild == null) return;

            bool newState = !wandChild.activeSelf;
            wandChild.SetActive(newState);

            Debug.Log(newState
                ? "🌕 Player toggled wand ON (failsafe)."
                : "🌑 Player toggled wand OFF (failsafe).");
        }
    }
}
