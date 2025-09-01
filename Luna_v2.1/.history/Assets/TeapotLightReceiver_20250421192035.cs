using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    public GameObject sparkleEffect;
    public GameObject brewingIndicatorIcon;

    private bool isLit       = false;
    private bool readyToBrew = false;

    /// <summary>
    /// Called by the wand when you press Q on the teapot.
    /// </summary>
    public void ActivateBrewReadyState()
    {
        if (isLit) return;
        isLit       = true;
        readyToBrew = true;

        sparkleEffect?.SetActive(true);
        brewingIndicatorIcon?.SetActive(true);

        Debug.Log("Teapot is ready to brew!");
    }

    /// <summary>
    /// Polled by TeaStateManager to know if there’s light.
    /// </summary>
    public bool IsReadyToBrew()
    {
        return readyToBrew;
    }

    /// <summary>
    /// Polled by TeaStateManager to know if there are any ingredients.
    /// You said you only care about lighting, so always false for now.
    /// </summary>
    public bool HasAnyIngredients()
    {
        return false;
    }

    /// <summary>
    /// Brew the tea into a teacup GameObject.
    /// Return your teacup prefab instance (or null if something went wrong).
    /// </summary>
    public GameObject BrewTea()
    {
        // TODO: Instantiate your teacup here.
        // For now we’ll just return null so you’ll get the warning in TeaStateManager.
        return null;
    }

    /// <summary>
    /// Optional: if you still use trigger‐based lighting elsewhere.
    /// </summary>
    public void ReceiveLight()
    {
        Debug.Log("TeapotLightReceiver – light received!");
    }

    public void ResetTeapot()
    {
        isLit       = false;
        readyToBrew = false;

        if (sparkleEffect         != null) sparkleEffect        .SetActive(false);
        if (brewingIndicatorIcon  != null) brewingIndicatorIcon .SetActive(false);
    }
}
