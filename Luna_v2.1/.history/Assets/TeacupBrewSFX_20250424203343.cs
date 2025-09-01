using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    void Start()
    {
        var source = GetComponent<AudioSource>();
        if (source != null && source.clip != null)
            source.Play();
    }
}
