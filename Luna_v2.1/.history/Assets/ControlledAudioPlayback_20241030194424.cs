using UnityEngine;

public class ControlledAudioPlayback : MonoBehaviour
{
    public AudioSource audioSource;  // Reference to the AudioSource component
    public float startTime = 0f;     // The time in seconds to start playback
    public float endTime = 5f;       // The time in seconds to stop playback

    private void Start()
    {
        // Ensure the audio source and clip are available
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("AudioSource or AudioClip is missing.");
            return;
        }

        // Ensure start and end times are within the clip length
        float clipLength = audioSource.clip.length;
        if (startTime < 0 || startTime >= clipLength || endTime <= startTime || endTime > clipLength)
        {
            Debug.LogError($"Invalid start or end time. Clip length is {clipLength} seconds.");
            return;
        }

        // Set the starting time and play the audio
        audioSource.time = startTime;
        audioSource.Play();
        Debug.Log($"Playing audio from {startTime} seconds to {endTime} seconds.");
    }

    private void Update()
    {
        // Check if the audio should stop based on the endTime
        if (audioSource != null && audioSource.isPlaying)
        {
            if (audioSource.time >= endTime)
            {
                audioSource.Stop();
                Debug.Log("Audio stopped at endTime.");
            }
        }
    }
}
