// SproutAndLightManager.cs
using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public bool isPlanted = false;

    [Header("Growth Settings")]
    [Tooltip("Seconds from planting until mature")]
    public float growDuration = 5f;
    private float growTimer = 0f;
    private bool isMature = false;

    [Header("Light‑Mote Settings")]
    [Tooltip("Prefab of the light mote to spawn")]
    public GameObject lightMotePrefab;
    [Tooltip("How often (seconds) to emit motes once mature")]
    public float moteInterval = 3f;
    private float moteTimer = 0f;

    [Header("Visuals")]
    [Tooltip("Effect to show when mature")]
    public GameObject glowEffect;
    [Tooltip("Effect to show while growing (optional)")]
    public GameObject growthEffect;

    void Update()
    {
        if (isPlanted && !isHeld)
        {
            HandleGrowth();
            if (isMature)
                HandleMoteEmission();
        }
        UpdateVisuals();
    }

    private void HandleGrowth()
    {
        if (isMature)
            return;

        growTimer += Time.deltaTime;
        if (growTimer >= growDuration)
        {
            isMature = true;
            // stop growthEffect if you have one
            if (growthEffect != null)
                growthEffect.SetActive(false);
        }
    }

    private void HandleMoteEmission()
    {
        if (lightMotePrefab == null)
            return;

        moteTimer += Time.deltaTime;
        if (moteTimer >= moteInterval)
        {
            // spawn a mote at the sprout’s position
            Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
            moteTimer = 0f;
        }
    }

    private void UpdateVisuals()
    {
        if (glowEffect != null)
            glowEffect.SetActive(isMature && !isHeld);
        if (growthEffect != null)
            growthEffect.SetActive(isPlanted && !isMature);
    }

    /// <summary>
    /// (Optional) if you want to force‑mature early:
    /// </summary>
    public void MatureNow()
    {
        isMature = true;
        growTimer = growDuration;
        if (growthEffect != null)
            growthEffect.SetActive(false);
    }
}
