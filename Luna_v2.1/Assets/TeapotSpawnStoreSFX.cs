using UnityEngine;

public class TeapotSpawnStoreSFX : MonoBehaviour
{
    public AudioSource spawnAudioSource;
    public AudioSource storeAudioSource;

    public void PlaySpawnSFX()
    {
        Debug.Log("PlaySpawnSFX called! " + (spawnAudioSource != null ? "AudioSource is set." : "AudioSource is null."));
        if (spawnAudioSource != null)
        {
            spawnAudioSource.Play();
            Debug.Log("Spawn SFX play requested. Clip: " + (spawnAudioSource.clip != null ? spawnAudioSource.clip.name : "NO CLIP"));
        }
    }

    public void PlayStoreSFX()
    {
        Debug.Log("PlayStoreSFX called! " + (storeAudioSource != null ? "AudioSource is set." : "AudioSource is null."));
        if (storeAudioSource != null)
        {
            storeAudioSource.Play();
            Debug.Log("Store SFX play requested. Clip: " + (storeAudioSource.clip != null ? storeAudioSource.clip.name : "NO CLIP"));
        }
    }
}
