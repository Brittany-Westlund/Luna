using UnityEngine;

public class PlayOnStart : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
