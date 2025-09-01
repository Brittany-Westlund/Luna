using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayBrewSound()
    {
        Debug.Log("[TeacupBrewSFX] PlayBrewSound called!");
        if (source != null && source.clip != null)
        {
            source.Play();
        }
        else
        {
            Debug.LogWarning("[TeacupBrewSFX] AudioSource or clip missing!");
        }
    }
}
