using UnityEngine;
using System.Collections;

public class ButtercupPollenPickup : MonoBehaviour
{
    [Header("Fatigue")]
    public int fatigueReductionAmount = 1;

    [Header("Animator Speed Boost")]
    [Tooltip("If true, temporarily increases the butterfly animator speed on collection.")]
    public bool boostAnimatorSpeed = true;

    [Tooltip("Temporary animator speed while boosted.")]
    public float boostedAnimatorSpeed = 2f;

    [Tooltip("How long the animator speed boost lasts.")]
    public float animatorBoostDuration = 2f;

    [Tooltip("If left empty, the script will use the Animator on the colliding object.")]
    public Animator overrideAnimator;

    [Header("Auto-Find Butterfly Pollen Icon")]
    [Tooltip("Name of the butterfly child object to show when pollen is collected.")]
    public string pollenIconObjectName = "ButtercupPollen";

    [Tooltip("How long the butterfly pollen icon stays visible.")]
    public float pollenIconDuration = 2f;

    [Header("Debug")]
    public bool debugLogs = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Butterfly"))
            return;

        // Optional fatigue reduction
        ButterflyFatigue fatigue = other.GetComponent<ButterflyFatigue>();
        if (fatigue != null)
        {
            fatigue.ReduceFatigue(fatigueReductionAmount);

            if (debugLogs)
                Debug.Log($"[ButtercupPollenPickup] Reduced fatigue by {fatigueReductionAmount}.");
        }

        // Optional companion-butterfly handler
        ButterflyFlyHandler flyHandler = other.GetComponent<ButterflyFlyHandler>();
        if (flyHandler != null)
        {
            flyHandler.ShowButtercupPollenIcon();

            if (debugLogs)
                Debug.Log("[ButtercupPollenPickup] Triggered ButterflyFlyHandler pollen icon.");
        }

        // Optional animator speed boost
        if (boostAnimatorSpeed)
        {
            Animator targetAnimator = overrideAnimator != null
                ? overrideAnimator
                : other.GetComponent<Animator>();

            if (targetAnimator == null)
                targetAnimator = other.GetComponentInChildren<Animator>();

            if (targetAnimator != null)
            {
                AnimatorSpeedBoostRunner runner = targetAnimator.GetComponent<AnimatorSpeedBoostRunner>();
                if (runner == null)
                    runner = targetAnimator.gameObject.AddComponent<AnimatorSpeedBoostRunner>();

                runner.ApplyBoost(targetAnimator, boostedAnimatorSpeed, animatorBoostDuration);

                if (debugLogs)
                    Debug.Log($"[ButtercupPollenPickup] Boosted animator speed to {boostedAnimatorSpeed} for {animatorBoostDuration} seconds.");
            }
            else if (debugLogs)
            {
                Debug.LogWarning("[ButtercupPollenPickup] No Animator found to boost.");
            }
        }

        // Auto-find the butterfly's child pollen icon by name
        if (!string.IsNullOrEmpty(pollenIconObjectName))
        {
            Transform iconTransform = FindChildRecursive(other.transform, pollenIconObjectName);

            if (iconTransform != null)
            {
                PollenVisualRunner visualRunner = iconTransform.GetComponent<PollenVisualRunner>();
                if (visualRunner == null)
                    visualRunner = iconTransform.gameObject.AddComponent<PollenVisualRunner>();

                visualRunner.ShowForDuration(pollenIconDuration);

                if (debugLogs)
                    Debug.Log($"[ButtercupPollenPickup] Found and showed pollen icon '{pollenIconObjectName}' for {pollenIconDuration} seconds.");
            }
            else if (debugLogs)
            {
                Debug.LogWarning($"[ButtercupPollenPickup] Could not find butterfly child named '{pollenIconObjectName}'.");
            }
        }

        Destroy(gameObject);
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == targetName)
                return child;

            Transform found = FindChildRecursive(child, targetName);
            if (found != null)
                return found;
        }

        return null;
    }
}

public class AnimatorSpeedBoostRunner : MonoBehaviour
{
    private Coroutine currentBoostRoutine;

    public void ApplyBoost(Animator targetAnimator, float boostedSpeed, float duration)
    {
        if (targetAnimator == null)
            return;

        if (currentBoostRoutine != null)
            StopCoroutine(currentBoostRoutine);

        currentBoostRoutine = StartCoroutine(BoostRoutine(targetAnimator, boostedSpeed, duration));
    }

    private IEnumerator BoostRoutine(Animator targetAnimator, float boostedSpeed, float duration)
    {
        float originalSpeed = targetAnimator.speed;
        targetAnimator.speed = boostedSpeed;

        yield return new WaitForSeconds(duration);

        if (targetAnimator != null)
            targetAnimator.speed = originalSpeed;

        currentBoostRoutine = null;
    }
}

public class PollenVisualRunner : MonoBehaviour
{
    private Coroutine currentRoutine;
    private SpriteRenderer[] cachedRenderers;

    private void Awake()
    {
        CacheRenderers();
        SetRenderersVisible(false);
    }

    public void ShowForDuration(float duration)
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
            CacheRenderers();

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        SetRenderersVisible(true);

        yield return new WaitForSeconds(duration);

        SetRenderersVisible(false);

        currentRoutine = null;
    }

    private void CacheRenderers()
    {
        cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void SetRenderersVisible(bool visible)
    {
        if (cachedRenderers == null)
            return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
                cachedRenderers[i].enabled = visible;
        }
    }
}