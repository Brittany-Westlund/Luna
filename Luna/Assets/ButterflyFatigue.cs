using UnityEngine;

public class ButterflyFatigue : MonoBehaviour
{
    [Header("Butterfly Components")]
    public SpriteRenderer butterflyRenderer;
    public Animator butterflyAnimator;

    [Header("Fatigue Settings")]
    public Color normalColor = Color.white;
    public Color maxFatigueColor = new Color(1f, 0.6f, 0.7f);
    public float normalAnimationSpeed = 1f;
    public float minAnimationSpeed = 0.5f;
    public int maxFatigueSteps = 5;

    [Header("Buttercup Progression")]
    public int fatigueStepCap = 10;

    [Header("Exhaustion FX")]
    public GameObject exhaustionIcon;

    private int currentFatigue = 0;
    private int buttercupPollenCount = 0;
    public bool isExhausted = false;

    void Start()
    {
        if (butterflyRenderer == null)
            butterflyRenderer = GetComponent<SpriteRenderer>();

        if (butterflyAnimator == null)
            butterflyAnimator = GetComponent<Animator>();

        ResetFatigue();
    }

    public void ApplyFatigue(bool affectColor = true)
    {
        if (isExhausted)
            return;

        currentFatigue = Mathf.Clamp(currentFatigue + 1, 0, maxFatigueSteps);

        float fatigueRatio = GetFatigueRatio();

        if (affectColor && butterflyRenderer != null)
            butterflyRenderer.color = Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);

        if (butterflyAnimator != null)
            butterflyAnimator.speed = Mathf.Lerp(normalAnimationSpeed, minAnimationSpeed, fatigueRatio);

        if (currentFatigue >= maxFatigueSteps)
        {
            isExhausted = true;
            Debug.Log("🦋 Exhausted");

            if (exhaustionIcon != null)
                exhaustionIcon.SetActive(true);
        }
    }

    public void ReduceFatigue(int amount)
    {
        currentFatigue = Mathf.Clamp(currentFatigue - Mathf.Abs(amount), 0, maxFatigueSteps);

        if (currentFatigue < maxFatigueSteps)
            isExhausted = false;

        RefreshVisuals();

        if (exhaustionIcon != null)
            exhaustionIcon.SetActive(isExhausted);
    }

    public void ReduceFatigueFullyAndApplyButtercupProgress()
    {
        currentFatigue = 0;
        buttercupPollenCount++;

        if (buttercupPollenCount >= 5 && maxFatigueSteps < fatigueStepCap)
        {
            buttercupPollenCount = 0;
            maxFatigueSteps++;
            Debug.Log($"🌼 Buttercup Level Up! New maxFatigueSteps: {maxFatigueSteps}");
        }

        isExhausted = false;
        RefreshVisuals();

        if (exhaustionIcon != null)
            exhaustionIcon.SetActive(false);
    }

    public void ResetFatigue()
    {
        currentFatigue = 0;
        isExhausted = false;
        RefreshVisuals();

        if (exhaustionIcon != null)
            exhaustionIcon.SetActive(false);
    }

    public bool IsExhausted()
    {
        return isExhausted;
    }

    public bool WouldExceedFatigue()
    {
        return currentFatigue + 1 > maxFatigueSteps;
    }

    public int GetCurrentFatigue()
    {
        return currentFatigue;
    }

    public int GetMaxFatigueSteps()
    {
        return maxFatigueSteps;
    }

    public float GetFatigueRatio()
    {
        if (maxFatigueSteps <= 0)
            return 0f;

        return (float)currentFatigue / maxFatigueSteps;
    }

    public Color GetCurrentFatigueColor()
    {
        return Color.Lerp(normalColor, maxFatigueColor, GetFatigueRatio());
    }

    public Color GetFatigueColor()
    {
        return GetCurrentFatigueColor();
    }

    public Color ApplyFatigueAndReturnColor()
    {
        ApplyFatigue();
        return GetCurrentFatigueColor();
    }

    private void RefreshVisuals()
    {
        float fatigueRatio = GetFatigueRatio();

        if (butterflyRenderer != null)
            butterflyRenderer.color = Color.Lerp(normalColor, maxFatigueColor, fatigueRatio);

        if (butterflyAnimator != null)
            butterflyAnimator.speed = Mathf.Lerp(normalAnimationSpeed, minAnimationSpeed, fatigueRatio);
    }
}