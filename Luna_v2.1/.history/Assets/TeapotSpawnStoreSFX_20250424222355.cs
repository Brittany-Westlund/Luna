using UnityEngine;

public class TeapotSpawnStoreSFX : MonoBehaviour
{
    public AudioSource spawnAudioSource;
    public AudioSource storeAudioSource;

    public void PlaySpawnSFX()
    {
        if (spawnAudioSource != null)
            spawnAudioSource.Play();
    }

    public void PlayStoreSFX()
    {
        if (storeAudioSource != null)
            storeAudioSource.Play();
    }
}
