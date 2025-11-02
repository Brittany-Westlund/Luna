using UnityEngine;

/// <summary>
/// 💫 Automatically disables the collider when the sprite fades out.
/// Designed to pair with LunariaGlowFromFlowerType.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LunariaColliderFade : MonoBehaviour
{
    [Header("References")]
    public LunariaGlowFromLightSources_Array lunaria; // optional link
    private SpriteRenderer sr;
    private BoxCollider2D boxCollider;

    [Header("Settings")]
    [Range(0f, 1f)] public float disableThreshold = 0.2f;
    public float checkInterval = 0.1f; // lightweight periodic check instead of every frame

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (lunaria == null)
            lunaria = GetComponent<LunariaGlowFromLightSources_Array>();

        if (sr == null || boxCollider == null)
        {
            Debug.LogError($"{name}: Missing SpriteRenderer or BoxCollider2D!");
            enabled = false;
            return;
        }

        StartCoroutine(CheckAlphaRoutine());
    }

    private System.Collections.IEnumerator CheckAlphaRoutine()
    {
        while (true)
        {
            float alpha = sr.color.a;

            bool shouldEnable = alpha >= disableThreshold;
            if (boxCollider.enabled != shouldEnable)
                boxCollider.enabled = shouldEnable;

            yield return new WaitForSeconds(checkInterval);
        }
    }
}
