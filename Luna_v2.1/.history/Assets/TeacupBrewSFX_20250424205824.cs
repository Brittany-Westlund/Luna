using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    public AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    // Call this method when you want the sound to play!
    public void PlayBrewSound()
    {
        if (source != null && source.clip != null)
            source.Play();
    }
}
