using UnityEngine;
using System.Collections;

public class ControlledAudioPlayback : MonoBehaviour
{
    public AudioSource audioSource;  // Reference to the AudioSource component
    public float startTime = 0f;     // The time in seconds to start playback
    public float endTime = 5f;       // The time in seconds to stop playback

    private void Start()
    {
        // Start the audio playback coroutine
        StartCoroutine(PlayAudioWithControlledStop());
    }

    private IEnumerator PlayAudioWithControlledStop()
    {
        // Ensure the audio source and clip are available
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("AudioSource or AudioClip is missing.");
            yield break;
        }

        // Ensure start and end times are within the clip length
        float clipLength = audioSource.clip.length;
        if (startTime < 0 || startTime >= clipLength || endTime <= startTime || endTime > clipLength)
        {
            Debug.LogError($"Invalid start or end time. Clip length is {clipLength} seconds.");
            yield break;
        }

        // Set the start time and play the audio
        audioSource.time = startTime;
        audioSource.Play();

        // Calculate the play duration and wait for that time
        float playDuration = endTime - startTime;
        yield return new WaitForSeconds(playDuration);

        // Stop the audio after the specified duration
        audioSource.Stop();
    }
}
