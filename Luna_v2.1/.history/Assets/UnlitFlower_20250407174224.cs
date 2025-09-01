using UnityEngine;

public class UnlitFlower : MonoBehaviour
{
    [Header("Illumination Settings")]
    public GameObject litVersion;            // Drag the lit flower version here
    public GameObject lightHintIcon;         // Assign if you want this icon to signal readiness
    public float minReadyScale = 0.95f;      // Allow slight flexibility for max scale

    public bool IsReadyToIlluminate()
    {
        bool isMaxScale = transform.localScale.x >= minReadyScale;
        bool isIconOn = lightHintIcon != null && lightHintIcon.activeSelf;

        return isMaxScale || isIconOn;
    }

    public bool Illuminate()
    {
        if (!IsReadyToIlluminate()) return false;

        if (litVersion != null)
        {
            litVersion.SetActive(true);
            gameObject.SetActive(false);
            Debug.Log("🌼 Flower illuminated: " + gameObject.name);
            return true;
        }

        Debug.LogWarning("⚠️ No litVersion assigned for " + gameObject.name);
        return false;
    }
}
