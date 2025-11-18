using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoonbowLifetimeController : MonoBehaviour
{
    [Header("⏳ Lifetime Settings")]
    [Tooltip("Seconds the Moonbow stays active before fading.")]
    public float lifetimeSeconds = 6f;

    [Tooltip("Fade-out duration in seconds.")]
    public float fadeDuration = 2f;

    [Header("🌟 Light Source Settings")]
    [Tooltip("If true, a nearby light source will stop the moonbow from fading.")]
    public bool requiresLightToStay = false;

    [Tooltip("Radius to check for a LightSource tag or script.")]
    public float lightCheckRadius = 3f;

    [Tooltip("What objects count as 'light sources'?")]
    public LayerMask lightLayer;

    [Header("References")]
    public List<SpriteRenderer> spritesToFade = new List<SpriteRenderer>();
    public Collider2D platformCollider;

    private bool isFading = false;

    private void OnEnable()
    {
        if (platformCollider == null) 
            platformCollider = GetComponent<Collider2D>();

        if (spritesToFade.Count == 0)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                spritesToFade.Add(sr);
        }

        // start the timer
        StopAllCoroutines();
        StartCoroutine(LifetimeRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        float timer = 0f;

        while (timer < lifetimeSeconds)
        {
            // If Moonbow must check for light
            if (requiresLightToStay && IsNearLightSource())
            {
                timer = 0f; // reset timer while light is close
            }
            else
            {
                timer += Time.deltaTime;
            }

            yield return null;
        }

        StartCoroutine(FadeOut());
    }

    private bool IsNearLightSource()
    {
        // checks for objects in the specified layer
        Collider2D light = Physics2D.OverlapCircle(
            transform.position,
            lightCheckRadius,
            lightLayer
        );

        return light != null;
    }

    private IEnumerator FadeOut()
    {
        if (isFading) yield break;
        isFading = true;

        float t = 0f;
        List<float> startAlphas = new List<float>();

        foreach (var sr in spritesToFade)
            startAlphas.Add(sr.color.a);

        // fade sprites
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            for (int i = 0; i < spritesToFade.Count; i++)
            {
                if (spritesToFade[i] == null) continue;

                Color c = spritesToFade[i].color;
                c.a = Mathf.Lerp(startAlphas[i], 0f, normalized);
                spritesToFade[i].color = c;
            }

            yield return null;
        }

        // disable platform
        if (platformCollider != null)
            platformCollider.enabled = false;

        // destroy or disable the moonbow completely
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (requiresLightToStay)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lightCheckRadius);
        }
    }
}
