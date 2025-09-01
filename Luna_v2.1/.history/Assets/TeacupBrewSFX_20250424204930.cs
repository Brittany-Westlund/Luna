using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    void Start()
    {
        var source = GetComponent<AudioSource>();
        if (source != null && source.clip != null)
        {
            // Defensive: Only play if not already playing
            if (!source.isPlaying)
                source.Play();
        }
    }
}
