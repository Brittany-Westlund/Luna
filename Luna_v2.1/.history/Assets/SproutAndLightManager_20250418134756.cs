using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

[RequireComponent(typeof(FlowerPickup))]
public class SproutAndLightManager : MonoBehaviour
{
    [Header("Growth Settings")]    
    public float growthIncrement   = 0.1f;
    public float yPositionIncrement= 0.04f;
    public float maxScale          = 0.3f;
    public float maxHeight         = 1.8f;

    [Header("Light Activation")]
    public float lightActivationCost = 0.1f;

    [Header("Spore Hint Icons")]
    public GameObject sporeHintPrefab;
    public Vector3    hintOffsetStage1 = new Vector3(0,1.5f,0);
    public Vector3    hintOffsetStage2 = new Vector3(0,1.1f,0);
    public Vector3    hintOffsetStage3 = new Vector3(0,0.85f,0);
    public float      hintScaleStage1 = 1f;
    public float      hintScaleStage2 = 0.75f;
    public float      hintScaleStage3 = 0.6f;

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset = new Vector3(0,0.2f,0);
    public float      lightHintScale  = 0.6f;

    // ← back‑compat fields:
    [HideInInspector] public bool isPlayerNearby = false;

    private FlowerPickup pickup;
    private MMProgressBar LightBar;
    private Text PointsText;
    private SpriteRenderer LitRenderer;
    private FlowerSway flowerSway;

    private bool isFullyGrown = false;
    private GameObject currentSporeHint;
    private GameObject currentLightHint;
    private Coroutine hintCoroutine;

    // expose for old code:
    public bool isHeld    => pickup.IsHeld;
    public bool isPlanted => pickup.IsPlanted;

    void Awake()
    {
        pickup = GetComponent<FlowerPickup>();
    }

    void Start()
    {
        // find your bar & points
        var lb = GameObject.FindWithTag("LightBar");
        if (lb != null) LightBar = lb.GetComponent<MMProgressBar>();
        if (ScoreManager.Instance != null)
            PointsText = ScoreManager.Instance.PointsText;

        // use our own helper instead of extension:
        var litChild = FindDeepChild(transform, "LitFlowerB");
        if (litChild != null) LitRenderer = litChild.GetComponent<SpriteRenderer>();

        flowerSway = GetComponent<FlowerSway>();

        if (LitRenderer != null) LitRenderer.enabled = false;
        if (flowerSway != null)  flowerSway.enabled = false;
        if (PointsText != null)  PointsText.enabled = false;
    }

    void Update()
    {
        // if someone’s holding or it's planted → stop hints/grow
        if (pickup.IsHeld || pickup.IsPlanted)
        {
            HideSporeHint();
            return;
        }

        // spore hint logic
        if (isPlayerNearby && !isFullyGrown)
        {
            if (hintCoroutine == null)
                hintCoroutine = StartCoroutine(ShowSporeHintDelayed());
        }
        else HideSporeHint();

        // light activation
        if (Input.GetButtonDown("Player1_LightActivation")
            && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }
    }

    private IEnumerator ShowSporeHintDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        ShowSporeHint();
        hintCoroutine = null;
    }

    private void ShowSporeHint()
    {
        float s = transform.localScale.x;
        Vector3 off;
        float sc;

        if (s < 0.1f)         { off = hintOffsetStage1; sc = hintScaleStage1; }
        else if (s < 0.2f)    { off = hintOffsetStage2; sc = hintScaleStage2; }
        else if (s < maxScale){ off = hintOffsetStage3; sc = hintScaleStage3; }
        else
        {
            HideSporeHint();
            return;
        }

        if (currentSporeHint == null)
            currentSporeHint = Instantiate(sporeHintPrefab,
                                           transform.position + off,
                                           Quaternion.identity,
                                           transform);

        currentSporeHint.transform.localPosition = off;
        currentSporeHint.transform.localScale   = Vector3.one * sc;
    }

    private void HideSporeHint()
    {
        if (currentSporeHint != null) Destroy(currentSporeHint);
        currentSporeHint = null;
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
    }

    public void ResetOnGrowth()
    {
        if (pickup.IsHeld || pickup.IsPlanted || isFullyGrown) return;
        float next = transform.localScale.x + growthIncrement;
        if (next >= maxScale) StartCoroutine(GrowToMax());
        else                  StartCoroutine(GrowStep());
    }

    private IEnumerator GrowStep()
    {
        var startS = transform.localScale;
        var endS   = new Vector3(
            Mathf.Min(startS.x + growthIncrement, maxScale),
            Mathf.Min(startS.y + growthIncrement, maxScale),
            1f);

        var startP = transform.position;
        var endP   = new Vector3(
            startP.x,
            Mathf.Min(startP.y + yPositionIncrement, maxHeight),
            startP.z);

        float t = 0f, d = 0.15f;
        HideSporeHint();

        while (t < d)
        {
            float f = t / d;
            transform.localScale = Vector3.Lerp(startS, endS, f);
            transform.position   = Vector3.Lerp(startP, endP, f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endS;
        transform.position   = endP;

        yield return new WaitForSeconds(0.05f);
        if (isPlayerNearby) ShowSporeHint();
    }

    private IEnumerator GrowToMax()
    {
        yield return GrowStep();
        FullyGrow();
    }

    private void FullyGrow()
    {
        isFullyGrown = true;
        if (flowerSway != null) flowerSway.enabled = true;

        if (lightHintPrefab != null)
        {
            currentLightHint = Instantiate(lightHintPrefab,
                                           transform.position + lightHintOffset,
                                           Quaternion.identity,
                                           transform);
            currentLightHint.transform.localScale = Vector3.one * lightHintScale;
        }
    }

    private void ActivateLight()
    {
        if (LightBar == null || ScoreManager.Instance == null) return;
        if (LightBar.BarProgress < lightActivationCost) return;

        LightBar.UpdateBar01(LightBar.BarProgress - lightActivationCost);
        if (LitRenderer != null) LitRenderer.enabled = true;
        if (PointsText  != null) PointsText.enabled = true;
        ScoreManager.Instance.AddPoint();

        if (currentLightHint != null)
        {
            Destroy(currentLightHint);
            currentLightHint = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (!isFullyGrown && flowerSway != null)
                flowerSway.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (!isFullyGrown && flowerSway != null)
                flowerSway.enabled = false;
        }
    }

    // Recursively search for a child by name
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
