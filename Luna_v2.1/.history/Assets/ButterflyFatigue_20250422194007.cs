using UnityEngine;

public class ButterflyFatigue : MonoBehaviour
{
    [Header("Butterfly Components")]
    public SpriteRenderer butterflyRenderer; // Reference to the butterfly sprite
    public Animator butterflyAnimator; // Reference to the animator

    [Header("Fatigue Settings")]
    public Color normalColor = Color.white; // Default color
    public Color maxFatigueColor = new Color(1f, 0.6f, 0.7f); // The cooldown pink color
    public float normalAnimationSpeed = 1f; // Normal animation speed
    public float minAnimationSpeed = 0.5f; // Slowest animation speed
    public int maxFatigueSteps = 5; // Number of steps before full fatigue

    private int currentFatigue = 0; // Tracks fatigue level
    public bool isExhausted = false; // If true, flight is disabled

    private int buttercupPollenCount = 0;
    private int baseMaxFatigueSteps = 5;
    private int fatigueStepCap = 10;
    [Header("Exhaustion FX")]
    public GameObject exhaustionIcon;  // drag in the icon from the inspector


    void Start()
    {
        // Ensure references are set
        if (butterflyRenderer == null) butterflyRenderer = GetComponent<SpriteRenderer>();
        if (butterflyAnimator == null) butterflyAnimator = GetComponent<Animator>();

        ResetFatigue();
    }

    public void ApplyFatigue(bool affectColor = true)
    {
        if (isExhausted) return;

        currentFatigue = Mathf.Clamp(currentFatigue + 1, 0, maxFatigueSteps);

        float fatigueRatio = (float)currentFatigue / maxFatigueSteps;

        if (affectColor)
            butterflyRenderer.color = Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);

        butterflyAnimator.speed = Mathf.Lerp(normalAnimationSpeed, minAnimationSpeed, fatigueRatio);

        if (currentFatigue >= maxFatigueSteps)
        {
            isExhausted = true;
            Debug.Log("🦋 Exhausted — flight disabled");

            // Don't disable the FlyHandler directly — let it auto-dismount in ButterflyFlyHandler
            if (currentFatigue >= maxFatigueSteps)
        {
            isExhausted = true;
            Debug.Log("🦋 Exhausted — flight disabled");

            if (exhaustionIcon != null)
                exhaustionIcon.SetActive(true);
        }

        }
    }

    public Color GetCurrentFatigueColor()
    {
        float fatigueRatio = (float)currentFatigue / maxFatigueSteps;
        return Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);
    }

    private void DisableFlight()
    {
        Debug.Log("Butterfly is fully exhausted! Flight is disabled.");
        // You'll need to reference your flight script here and disable the flight input (F key)
        ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
        if (flyHandler != null)
        {
            flyHandler.enabled = false;
        }
    }

    public bool IsExhausted()
    {
        return isExhausted;
    }

    public void ResetFatigue()
    {
        currentFatigue = 0;
        isExhausted = false;
        butterflyRenderer.color = normalColor;
        if (butterflyAnimator != null) butterflyAnimator.speed = normalAnimationSpeed;

        // Re-enable flight
        ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
        if (flyHandler != null)
        {
            flyHandler.enabled = true;
        }

        if (exhaustionIcon != null)
        exhaustionIcon.SetActive(false);

    }

    public void ReduceFatigue(int amount)
    {
        if (currentFatigue <= 0) return;

        currentFatigue = Mathf.Max(0, currentFatigue - amount);
        buttercupPollenCount++;

        float fatigueRatio = (float)currentFatigue / maxFatigueSteps;

        if (buttercupPollenCount >= 5 && maxFatigueSteps < fatigueStepCap)
        {
            buttercupPollenCount = 0;

            maxFatigueSteps++;
            currentFatigue = Mathf.RoundToInt(fatigueRatio * maxFatigueSteps);

            Debug.Log($"🌼 Buttercup Level Up! New maxFatigueSteps: {maxFatigueSteps}, adjusted currentFatigue: {currentFatigue}");
        }

        // Update visuals
        butterflyRenderer.color = GetFatigueColor();
        butterflyAnimator.speed = Mathf.Lerp(normalAnimationSpeed, minAnimationSpeed, fatigueRatio);

        // Recover from exhaustion
        if (isExhausted && currentFatigue < maxFatigueSteps)
        {
            isExhausted = false;
            ButterflyFlyHandler flyHandler = GetComponent<ButterflyFlyHandler>();
            if (flyHandler != null)
                flyHandler.enabled = true;
        }
    }

    public Color GetFatigueColor()
    {
        float fatigueRatio = (float)currentFatigue / maxFatigueSteps;
        return Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);
    }

    public Color ApplyFatigueAndReturnColor()
    {
        ApplyFatigue();
        return GetFatigueColor();
    }

    public bool WouldExceedFatigue()
    {
        return currentFatigue + 1 > maxFatigueSteps;
    }

}
