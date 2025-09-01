using UnityEngine;

public class TeapotSpawnStoreSFX : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource spawnAudioSource; // Drag the AudioSource for spawn SFX
    public AudioSource storeAudioSource; // Drag the AudioSource for store SFX

    // Call this when the teapot spawns
    public void PlaySpawnSFX()
    {
        if (spawnAudioSource != null)
            spawnAudioSource.Play();
    }

    // Call this just before the teapot is destroyed/stored
    public void PlayStoreSFX()
    {
        if (storeAudioSource != null)
            storeAudioSource.Play();
    }
}
