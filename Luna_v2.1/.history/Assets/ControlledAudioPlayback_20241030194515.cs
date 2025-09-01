using UnityEngine;

public class ControlledAudioPlayback : MonoBehaviour
{
    public AudioSource audioSource;  // Reference to the AudioSource component
    public float startTime = 0f;     // The time in seconds to start playback
    public float endTime = 5f;       // The time in seconds to stop playback

    private void Start()
    {
        // Set the starting time and play the audio
        if (audioSource != null)
        {
            audioSource.time = startTime;
            audioSource.Play();
        }
    }

    private void Update()
    {
        // Check if the audio should stop based on the endTime
        if (audioSource != null && audioSource.isPlaying)
        {
            if (audioSource.time >= endTime)
            {
                audioSource.Stop();
            }
        }
    }
}
