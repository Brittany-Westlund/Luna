using UnityEngine;

public class TeacupDrinkSFX : MonoBehaviour
{
    public AudioSource drinkAudioSource;

    // Call this when tea is actually drunk
    public void PlayDrinkSFX()
    {
        if (drinkAudioSource != null && drinkAudioSource.clip != null)
            drinkAudioSource.Play();
    }
}
