using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    [Tooltip("Assign your sparkle effect child here")]
    public GameObject sparkleEffect;
    [Tooltip("Assign your brewing indicator child here")]
    public GameObject brewingIndicatorIcon;

    bool _isLit       = false;
    bool _readyToBrew = false;

    void Awake()
    {
        // ensure they start off
        if (sparkleEffect != null)         sparkleEffect       .SetActive(false);
        if (brewingIndicatorIcon != null)  brewingIndicatorIcon.SetActive(false);
    }

    /// <summary>
    /// Call from your wand script when Q‐delivered.
    /// </summary>
    public void ActivateBrewReadyState()
    {
        if (_isLit) return;
        _isLit       = true;
        _readyToBrew = true;

        if (sparkleEffect      != null) sparkleEffect       .SetActive(true);
        if (brewingIndicatorIcon!= null) brewingIndicatorIcon.SetActive(true);

        Debug.Log("🫖 Teapot is ready to brew!");
    }

    public bool IsReadyToBrew() => _readyToBrew;

    public void ResetTeapot()
    {
        _isLit       = false;
        _readyToBrew = false;
        if (sparkleEffect      != null) sparkleEffect       .SetActive(false);
        if (brewingIndicatorIcon!= null) brewingIndicatorIcon.SetActive(false);
    }
}
