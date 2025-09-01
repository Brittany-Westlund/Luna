using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PointsSparkleEffect : MonoBehaviour
{
    public Text pointsText;                   // Reference to the PointsText UI element
    public GameObject sparkles;                // Reference to the Sparkles prefab
    public float holdDuration = 2f;            // Duration to hold the sparkles before fading
    public float fadeDuration = 1f;            // Duration to fade out the sparkles
    private int previousPoints = 0;            // Tracks the last recorded points value
    public AudioClip sparkleSound;       // Assign your sound in the inspector
    private AudioSource audioSource;     // Internal reference
    private void Start()
    {
        // Ensure sparkles are initially turned off
        if (sparkles != null)
        {
            sparkles.SetActive(false);
        }

        // Initialize previous points based on current PointsText value
        if (int.TryParse(pointsText.text, out int initialPoints))
        {
            previousPoints = initialPoints;
        }
    }

    private void Update()
    {
        // Check if the points have increased
        if (int.TryParse(pointsText.text, out int currentPoints) && currentPoints > previousPoints)
        {
            // Update the previous points to the current value
            previousPoints = currentPoints;

            // Trigger the sparkle effect with hold and fade out
            if (sparkles != null)
            {
                StartCoroutine(ActivateAndFadeSparkles());
            }
        }
    }

    private IEnumerator ActivateAndFadeSparkles()
    {
        // Turn on the Sparkles prefab
        sparkles.SetActive(true);

        // Get all SpriteRenderers in the Sparkles prefab
        SpriteRenderer[] sparkleRenderers = sparkles.GetComponentsInChildren<SpriteRenderer>();

        // Set initial full opacity
        foreach (SpriteRenderer renderer in sparkleRenderers)
        {
            Color color = renderer.color;
            color.a = 1f;  // Fully opaque
            renderer.color = color;
        }

        // Hold for the specified duration
        yield return new WaitForSeconds(holdDuration);

        // Gradually fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (SpriteRenderer renderer in sparkleRenderers)
            {
                Color color = renderer.color;
                color.a = alpha;
                renderer.color = color;
            }

            yield return null;
        }

        // Ensure all sparkles are fully transparent at the end
        foreach (SpriteRenderer renderer in sparkleRenderers)
        {
            Color color = renderer.color;
            color.a = 0f;
            renderer.color = color;
        }

        // Turn off the Sparkles prefab
        sparkles.SetActive(false);
    }
}
