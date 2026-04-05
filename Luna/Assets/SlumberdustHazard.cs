using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

[RequireComponent(typeof(Collider2D))]
public class SlumberdustHazard : MonoBehaviour
{
    [Header("Targeting")]
    public string playerTag = "Player";
    public string playerHeadName = "PlayerHead";

    [Header("Attachment Offset")]
    public Vector3 localOffset = new Vector3(0.5f, 0f, 0f);

    [Header("Slumberdust Effect")]
    [Range(0f, 1f)]
    public float healthReductionPercent = 0.25f;

    [Range(0f, 1f)]
    public float speedReductionPercent = 0.30f;

    public float duration = 3f;

    [Header("Fade + Destroy")]
    public float fadeOutDuration = 0.5f;

    [Header("Health Bar Auto-Find")]
    public string healthBarObjectName = "HealthBar";

    private bool hasTriggered = false;
    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        GameObject player = other.gameObject;

        LunaStatusEffects status = player.GetComponent<LunaStatusEffects>();
        if (status != null && status.IsSlumberdustImmune)
        {
            hasTriggered = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            Debug.Log("🌿 Slumberdust blocked by immunity.");
            StartCoroutine(FadeThenDestroyAfterDuration(0f));
            return;
        }

        hasTriggered = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        Health playerHealth = player.GetComponent<Health>();
        CharacterHorizontalMovement movement = player.GetComponent<CharacterHorizontalMovement>();
        MMProgressBar healthBar = FindHealthBarDeep();

        float finalHealthReduction = Mathf.Clamp01(healthReductionPercent);
        float finalSpeedReduction = Mathf.Clamp01(speedReductionPercent);

        MoonflowerCrownController crown = player.GetComponentInChildren<MoonflowerCrownController>(true);
        if (crown != null)
        {
            float crownReduction = Mathf.Clamp01(crown.CurrentSlumberdustReductionPercent);

            finalHealthReduction *= (1f - crownReduction);
            finalSpeedReduction *= (1f - crownReduction);

            Debug.Log("🌙 Moonflower crown reduced slumberdust by " + (crownReduction * 100f) + "%");
        }

        SlumberdustStatusReceiver receiver = player.GetComponent<SlumberdustStatusReceiver>();
        if (receiver == null)
        {
            receiver = player.AddComponent<SlumberdustStatusReceiver>();
        }

        receiver.ApplySlumberdust(
            playerHealth,
            movement,
            healthBar,
            finalHealthReduction,
            finalSpeedReduction,
            duration
        );

        Transform head = FindChildRecursive(player.transform, playerHeadName);
        if (head != null)
        {
            transform.SetParent(head, false);
            transform.localPosition = localOffset;
        }
        else
        {
            Debug.LogWarning("[Slumberdust] PlayerHead not found.");
        }

        StartCoroutine(FadeThenDestroyAfterDuration(duration));
    }

    private IEnumerator FadeThenDestroyAfterDuration(float waitDuration)
    {
        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        yield return StartCoroutine(FadeOutAllVisuals());
        Destroy(gameObject);
    }

    private IEnumerator FadeOutAllVisuals()
    {
        if (fadeOutDuration <= 0f)
        {
            SetAlphaImmediate(0f);
            yield break;
        }

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);
        CanvasGroup[] canvases = GetComponentsInChildren<CanvasGroup>(true);

        float[] spriteAlphas = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                spriteAlphas[i] = sprites[i].color.a;
            }
        }

        float[] canvasAlphas = new float[canvases.Length];
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvasAlphas[i] = canvases[i].alpha;
            }
        }

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float alphaMultiplier = 1f - t;

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                {
                    Color c = sprites[i].color;
                    c.a = spriteAlphas[i] * alphaMultiplier;
                    sprites[i].color = c;
                }
            }

            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] != null)
                {
                    canvases[i].alpha = canvasAlphas[i] * alphaMultiplier;
                }
            }

            yield return null;
        }

        SetAlphaImmediate(0f);
    }

    private void SetAlphaImmediate(float alpha)
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
            {
                Color c = sprites[i].color;
                c.a = alpha;
                sprites[i].color = c;
            }
        }

        CanvasGroup[] canvases = GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvases[i].alpha = alpha;
            }
        }
    }

    private MMProgressBar FindHealthBarDeep()
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < all.Length; i++)
        {
            GameObject obj = all[i];

            if (obj == null)
            {
                continue;
            }

            if (obj.name != healthBarObjectName)
            {
                continue;
            }

            if (!obj.scene.IsValid())
            {
                continue;
            }

            MMProgressBar bar = obj.GetComponent<MMProgressBar>();
            if (bar != null)
            {
                return bar;
            }

            MMProgressBar[] children = obj.GetComponentsInChildren<MMProgressBar>(true);
            if (children.Length > 0)
            {
                return children[0];
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == targetName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, targetName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}

public class SlumberdustStatusReceiver : MonoBehaviour
{
    private Coroutine activeCoroutine;

    private CharacterHorizontalMovement movement;
    private float originalSpeed;
    private bool reduced = false;

    public void ApplySlumberdust(
        Health health,
        CharacterHorizontalMovement move,
        MMProgressBar bar,
        float healthPercent,
        float speedPercent,
        float duration
    )
    {
        if (health != null)
        {
            float damageAmount = health.MaximumHealth * Mathf.Clamp01(healthPercent);
            float newHealth = Mathf.Clamp(health.CurrentHealth - damageAmount, 0f, health.MaximumHealth);

            health.SetHealth(newHealth, gameObject);

            if (bar != null)
            {
                bar.UpdateBar(health.CurrentHealth, 0f, health.MaximumHealth);
            }
        }

        if (move != null)
        {
            movement = move;

            if (!reduced)
            {
                originalSpeed = movement.MovementSpeed;
                reduced = true;
            }

            float reducedSpeed = originalSpeed * (1f - Mathf.Clamp01(speedPercent));
            movement.MovementSpeed = reducedSpeed;

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            activeCoroutine = StartCoroutine(RestoreAfter(duration));
        }
    }

    private IEnumerator RestoreAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (movement != null)
        {
            movement.MovementSpeed = originalSpeed;
        }

        reduced = false;
        activeCoroutine = null;
    }

    private void OnDisable()
    {
        if (movement != null && reduced)
        {
            movement.MovementSpeed = originalSpeed;
        }

        reduced = false;
        activeCoroutine = null;
    }
}