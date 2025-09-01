using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public bool isPlanted = false;

    [Header("Growth Settings")]
    public float growDuration = 5f;
    private float growTimer = 0f;
    private bool isMature = false;

    [Header("Light‑Mote Settings")]
    public GameObject lightMotePrefab;
    public float moteInterval = 3f;
    private float moteTimer = 0f;

    [Header("Visuals")]
    public GameObject glowEffect;
    public GameObject growthEffect;

    void Update()
    {
        if (isPlanted && !isHeld)
        {
            if (!isMature)
            {
                growTimer += Time.deltaTime;
                if (growTimer >= growDuration)
                {
                    isMature = true;
                    if (growthEffect != null) growthEffect.SetActive(false);
                }
            }
            else
            {
                moteTimer += Time.deltaTime;
                if (lightMotePrefab != null && moteTimer >= moteInterval)
                {
                    Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
                    moteTimer = 0f;
                }
            }
        }

        if (glowEffect != null)   glowEffect.SetActive(isMature && !isHeld);
        if (growthEffect != null) growthEffect.SetActive(isPlanted && !isMature);
    }
}
