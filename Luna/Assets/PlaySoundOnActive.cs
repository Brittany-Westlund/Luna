using UnityEngine;

public class PlaySoundOnActive : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Play when object becomes active
        if (audioSource != null && audioSource.clip != null)
            audioSource.Play();
    }
}
