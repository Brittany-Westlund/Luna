using UnityEngine;

public class TeapotSpawnStoreSFX : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;        // Drag your AudioSource here
    public AudioClip spawnClip;            // Drag your spawn sfx here
    public AudioClip storeClip;            // Drag your store sfx here

    void Awake()
    {
        // Optionally auto-find AudioSource if not set
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Call this when the teapot spawns
    public void PlaySpawnSFX()
    {
        if (audioSource != null && spawnClip != null)
            audioSource.PlayOneShot(spawnClip);
    }

    // Call this just before the teapot is destroyed/stored
    public void PlayStoreSFX()
    {
        if (audioSource != null && storeClip != null)
            audioSource.PlayOneShot(storeClip);
    }
}
