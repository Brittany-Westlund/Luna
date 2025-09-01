using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    public GameObject sparkleEffect;         // Sparkle effect or lit visual
    public GameObject brewingIndicatorIcon;  // Icon that appears when ready to brew

    private bool isLit = false;
    private bool readyToBrew = false;

    /// <summary>
    /// Called by the wand when you press Q near the teapot.
    /// </summary>
    public void ActivateBrewReadyState()
    {
        if (isLit) return;
        isLit = true;
        readyToBrew = true;

        if (sparkleEffect != null)         sparkleEffect.SetActive(true);
        if (brewingIndicatorIcon != null)  brewingIndicatorIcon.SetActive(true);

        Debug.Log("🫖 Teapot is ready to brew!");
    }

    public bool IsReadyToBrew()    => readyToBrew;

    public void ResetTeapot()
    {
        isLit = false;
        readyToBrew = false;
        if (sparkleEffect != null)         sparkleEffect.SetActive(false);
        if (brewingIndicatorIcon != null)  brewingIndicatorIcon.SetActive(false);
    }
}
