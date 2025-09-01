using UnityEngine;

public class ButterflyFatigue : MonoBehaviour
{
    [Header("Butterfly Components")]
    public SpriteRenderer butterflyRenderer;
    public Animator butterflyAnimator;

    [Header("Fatigue Settings")]
    public Color normalColor = Color.white;
    public Color maxFatigueColor = new Color(1f, 0.6f, 0.7f);
    public Color cooldownColor = Color.gray;
    public float normalAnimationSpeed = 1f;
    public float minAnimationSpeed = 0.5f;
    public int maxFatigueSteps = 5;

    private int currentFatigue = 0;
    private bool isExhausted = false;
    private bool inCooldown = false;

    void Start()
    {
        if (butterflyRenderer == null) butterflyRenderer = GetComponent<SpriteRenderer>();
        if (butterflyAnimator == null) butterflyAnimator = GetComponent<Animator>();

        ApplyFatigueVisuals();
    }

    public void ApplyFatigue()
    {
        if (isExhausted) return;

        currentFatigue++;
        if (currentFatigue >= maxFatigueSteps)
        {
            isExhausted = true;
            DisableFlight();
        }
    }

    public void ApplyFatigueVisuals()
    {
        float fatigueRatio = Mathf.Clamp01((float)currentFatigue / maxFatigueSteps);
        butterflyRenderer.color = Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);
        butterflyAnimator.speed = Mathf.Lerp(normalAnimationSpeed, minAnimationSpeed, fatigueRatio);
    }

    public void ApplyCooldownVisuals()
    {
        inCooldown = true;
        butterflyRenderer.color = cooldownColor;
    }

    public void ExitCooldownAndApplyFatigueVisuals()
    {
        inCooldown = false;
        ApplyFatigueVisuals();
    }

    private void DisableFlight()
    {
        Debug.Log("Butterfly is fully exhausted! Flight is disabled.");
        ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
        if (flyHandler != null)
        {
            flyHandler.enabled = false;
        }
    }

    public void ResetFatigue()
    {
        currentFatigue = 0;
        isExhausted = false;
        ApplyFatigueVisuals();

        ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
        if (flyHandler != null)
        {
            flyHandler.enabled = true;
        }
    }

    public void ReduceFatigue(int amount)
    {
        if (currentFatigue <= 0) return;

        currentFatigue -= amount;
        currentFatigue = Mathf.Max(0, currentFatigue);

        if (isExhausted && currentFatigue < maxFatigueSteps)
        {
            isExhausted = false;
            ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
            if (flyHandler != null)
            {
                flyHandler.enabled = true;
            }
        }

        ApplyFatigueVisuals();
    }

    public int GetFatigueLevel()
    {
        return currentFatigue;
    }
}