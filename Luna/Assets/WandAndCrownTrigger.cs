using UnityEngine;

/// <summary>
/// 🌙 Trigger-based activator for wand and optional crown.
/// Acts like pressing Q (wand) and optionally C (crown).
/// </summary>
public class WandAndCrownTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WandForever wandForever;
    [SerializeField] private MoonflowerCrownController crownController;

    [Header("Wand Settings")]
    public bool activateWandOnEnter = true;
    public bool deactivateWandOnExit = false;

    [Header("Crown Settings")]
    public bool activateCrownOnEnter = false;
    public bool deactivateCrownOnExit = false;

    [Header("General")]
    public string playerTag = "Player";
    public bool triggerOnce = false;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool hasTriggered = false;

    private void Awake()
    {
        if (wandForever == null)
            wandForever = FindObjectOfType<WandForever>();

        if (crownController == null)
            crownController = FindObjectOfType<MoonflowerCrownController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && hasTriggered) return;

        if (activateWandOnEnter)
            SetWand(true);

        if (activateCrownOnEnter)
            SetCrown(true);

        hasTriggered = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (deactivateWandOnExit)
            SetWand(false);

        if (deactivateCrownOnExit)
            SetCrown(false);
    }

    private void SetWand(bool state)
    {
        if (wandForever == null) return;

        var wandChild = wandForever.wandChild;
        if (wandChild == null) return;

        wandChild.SetActive(state);

        if (debugLogs)
        {
            Debug.Log(state
                ? "🌕 Wand ON via trigger"
                : "🌑 Wand OFF via trigger");
        }
    }

    private void SetCrown(bool state)
    {
        if (crownController == null) return;

        if (state)
            crownController.EnableCrown();
        else
            crownController.DisableCrown();

        if (debugLogs)
        {
            Debug.Log(state
                ? "👑 Crown ON via trigger"
                : "🚫 Crown OFF via trigger");
        }
    }
}