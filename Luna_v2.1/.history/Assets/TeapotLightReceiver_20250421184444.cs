using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    // these will be found at runtime under the teapot GameObject
    GameObject _sparkleEffect;
    GameObject _brewingIndicator;

    bool isLit       = false;
    bool readyToBrew = false;

    void Awake()
    {
        // automatically find the two child objects by name
        _sparkleEffect    = transform.Find("SparkleEffect")?.gameObject;
        _brewingIndicator = transform.Find("BrewingIndicator")?.gameObject;

        // ensure they start inactive
        if (_sparkleEffect    != null) _sparkleEffect   .SetActive(false);
        if (_brewingIndicator != null) _brewingIndicator.SetActive(false);
    }

    /// <summary>
    /// Call this when the wand delivers its mote via Q.
    /// </summary>
    public void ActivateBrewReadyState()
    {
        if (isLit) return;
        isLit       = true;
        readyToBrew = true;

        if (_sparkleEffect    != null) _sparkleEffect   .SetActive(true);
        if (_brewingIndicator != null) _brewingIndicator.SetActive(true);

        Debug.Log("Teapot is ready to brew!");
    }

    /// <summary>
    /// External check to see if the teapot is lit.
    /// </summary>
    public bool IsReadyToBrew() => readyToBrew;

    /// <summary>
    /// Resets the teapot back to unlit (for replay or level reload).
    /// </summary>
    public void ResetTeapot()
    {
        isLit       = false;
        readyToBrew = false;

        if (_sparkleEffect    != null) _sparkleEffect   .SetActive(false);
        if (_brewingIndicator != null) _brewingIndicator.SetActive(false);
    }
}
