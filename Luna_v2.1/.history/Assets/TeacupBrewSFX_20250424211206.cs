using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    public void PlayBrewSound()
    {
        var source = GetComponent<AudioSource>();
        if (source != null && source.clip != null)
            source.Play();
    }
}
