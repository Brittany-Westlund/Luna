using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanternSoundTrigger : MonoBehaviour
{
    [Header("Lantern Settings")]
    public string[] lanternTags = { "IndoorLantern", "OutdoorLantern", "Lantern" }; // Tags for lanterns to monitor
    public AudioSource triggerSound;                 // Sound to play when any SpriteRenderer is enabled

    private HashSet<GameObject> processedLanterns = new HashSet<GameObject>(); // Track which lanterns are enabled

    [Header("Delay Settings")]
    public float initialCheckDelay = 2f; // Delay in seconds before checking lanterns
    private bool initializationComplete = false; // Tracks whether initialization is complete

    private void Start()
    {
        // Delay initialization slightly
        StartCoroutine(InitializeEnabledLanternsWithDelay());
    }

    private IEnumerator InitializeEnabledLanternsWithDelay()
    {
        Debug.Log("Initializing enabled lanterns...");
        yield return new WaitForSeconds(initialCheckDelay); // Wait for the delay

        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer lanternRenderer = lantern.GetComponent<SpriteRenderer>();

                if (lanternRenderer != null && lanternRenderer.enabled)
                {
                    // Add the lantern to the processed list without playing the sound
                    processedLanterns.Add(lantern);
                    
                }
            }
        }

        initializationComplete = true; // Mark initialization as complete
        Debug.Log("Initialization complete. Lantern monitoring started.");
    }

    private void Update()
    {
        // Skip checking lanterns until initialization is complete
        if (!initializationComplete) return;

        // Continuously check for newly enabled SpriteRenderers
        CheckLanterns();
    }

    private void CheckLanterns()
    {
        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer lanternRenderer = lantern.GetComponent<SpriteRenderer>();

                // Check if the SpriteRenderer is enabled and not yet processed
                if (lanternRenderer != null && lanternRenderer.enabled && !processedLanterns.Contains(lantern))
                {
                    Debug.Log($"Playing sound for newly enabled lantern: {lantern.name}");

                    // Play the trigger sound for each newly lit lantern
                    if (triggerSound != null && triggerSound.isPlaying == false) // Avoid overlapping sounds
                    {
                        triggerSound.Play();
                    }

                    // Mark this lantern as processed
                    processedLanterns.Add(lantern);
                }
            }
        }
    }
}
