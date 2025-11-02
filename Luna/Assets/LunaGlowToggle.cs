using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class LunaGlowToggle : MonoBehaviour
{
    [Header("Animator Controllers")]
    public RuntimeAnimatorController normalController;
    public RuntimeAnimatorController glowController;

    [Header("Sprite Renderer (for fade effect)")]
    public SpriteRenderer lunaSprite;

    [Header("Fade Settings")]
    [Tooltip("How long the fade takes, in seconds.")]
    public float fadeDuration = 1.5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Brightness boost color when glowing.")]
    public Color glowColor = new Color(1.2f, 1.2f, 1.5f, 1f);

    [Header("Input Settings")]
    public KeyCode glowKey = KeyCode.G;
    public float holdTime = 1.2f;

    private Animator animator;
    public bool isGlowing = false;
    private Coroutine fadeRoutine;
    private float keyHeldTime = 0f;
    private bool glowTriggered = false;
    private Color normalColor;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = normalController;

        if (lunaSprite == null)
            lunaSprite = GetComponentInChildren<SpriteRenderer>();

        normalColor = lunaSprite.color; // remember her normal color
    }

    void Update()
    {
        HandleGlowInput();
    }

    private void HandleGlowInput()
    {
        if (Input.GetKey(glowKey))
        {
            keyHeldTime += Time.deltaTime;
            if (!glowTriggered && keyHeldTime >= holdTime)
            {
                glowTriggered = true;
                ToggleGlow();
            }
        }
        else
        {
            keyHeldTime = 0f;
            glowTriggered = false;
        }
    }

    public void ToggleGlow()
    {
        isGlowing = !isGlowing;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTransition(isGlowing));
    }

    private IEnumerator FadeTransition(bool toGlow)
    {
        float elapsed = 0f;
        Color startColor = lunaSprite.color;
        Color endColor = toGlow ? glowColor : normalColor;

        // 🌙 Fade smoothly over real time
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = fadeCurve.Evaluate(t);

            lunaSprite.color = Color.Lerp(startColor, endColor, eased);
            yield return null;
        }

        // 🌕 Swap animator after fade completes
        animator.runtimeAnimatorController = toGlow ? glowController : normalController;

        // Ensure final color is exact
        lunaSprite.color = endColor;
    }

    public bool IsGlowing => isGlowing;

}
