using UnityEngine;
using System.Collections.Generic;

public class AdjustBackgroundBrightness : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] targetBackgrounds;          // Array of background objects to adjust
    public string[] lanternTags;                    // Tags for all types of lanterns
    public float brightnessIncreaseStep = 0.1f;    // Fixed increase towards white each activation

    [Header("Flower Settings")]
    public GameObject[] flowersInScene;             // Array of flowers already placed in the scene
    public AudioSource brightnessFullAudio;         // Audio source to play when full brightness is reached

    private HashSet<GameObject> litLanterns = new HashSet<GameObject>(); // Track lanterns that have already been processed
    private bool isFullyBright = false;             // Flag to prevent multiple triggers when fully bright

    private void Update()
    {
        // Stop processing if backgrounds are already fully bright
        if (isFullyBright) return;

        bool newLanternDetected = false;

        // Check each lantern tag for newly enabled lanterns
        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer lanternRenderer = lantern.GetComponent<SpriteRenderer>();

                // If the lantern is enabled and not yet processed, increase brightness
                if (lanternRenderer != null && lanternRenderer.enabled && !litLanterns.Contains(lantern))
                {
                    litLanterns.Add(lantern);  // Mark this lantern as processed
                    newLanternDetected = true; // Flag to indicate a new lantern was detected
                }
            }
        }

        // Only increase brightness if a new lantern was detected this frame
        if (newLanternDetected)
        {
            IncreaseBrightness();
        }

        // Check if all backgrounds have reached full brightness
        if (CheckFullBrightness())
        {
            isFullyBright = true;
            TriggerFullBrightnessFeedback();
        }
    }

    // Method to adjust the brightness of each background object by a fixed step
    private void IncreaseBrightness()
    {
        foreach (GameObject background in targetBackgrounds)
        {
            SpriteRenderer backgroundRenderer = background.GetComponent<SpriteRenderer>();

            // Ensure the background has a SpriteRenderer
            if (backgroundRenderer != null)
            {
                // Get the current color
                Color currentColor = backgroundRenderer.color;

                // Increase RGB values by the fixed step, clamping each at 1 (full white)
                float newR = Mathf.Min(currentColor.r + brightnessIncreaseStep, 1f);
                float newG = Mathf.Min(currentColor.g + brightnessIncreaseStep, 1f);
                float newB = Mathf.Min(currentColor.b + brightnessIncreaseStep, 1f);

                // Apply the new color with increased brightness
                backgroundRenderer.color = new Color(newR, newG, newB, currentColor.a);
            }
        }
    }

    // Check if all backgrounds have reached full brightness
    private bool CheckFullBrightness()
    {
        foreach (GameObject background in targetBackgrounds)
        {
            SpriteRenderer backgroundRenderer = background.GetComponent<SpriteRenderer>();
            if (backgroundRenderer != null)
            {
                Color currentColor = backgroundRenderer.color;

                // If any color channel is less than 1, return false
                if (currentColor.r < 1f || currentColor.g < 1f || currentColor.b < 1f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Trigger feedback when background is fully bright
    private void TriggerFullBrightnessFeedback()
    {
        Debug.Log("Background is fully bright! Triggering flower activation and playing sound.");

        // Play the audio if it's assigned
        if (brightnessFullAudio != null)
        {
            brightnessFullAudio.Play();
        }

        // Activate flowers
        ActivateFlowers();
    }

    // Activate flowers in the scene
    private void ActivateFlowers()
    {
        foreach (GameObject flower in flowersInScene)
        {
            if (flower != null)
            {
                flower.SetActive(true); // Enable the flower GameObject
            }
        }
    }
}
