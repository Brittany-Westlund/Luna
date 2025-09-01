using UnityEngine;

public class TeacupBrewSFX : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            Debug.Log("Manual play from Update");
            PlayBrewSound();
        }
    }

    public void PlayBrewSound()
    {
        var source = GetComponent<AudioSource>();
        Debug.Log($"PlayBrewSound called! source null? {source == null} | clip null? {(source?.clip == null)} | isActive: {gameObject.activeInHierarchy}");
        if (source != null && source.clip != null)
            source.Play();
    }

}
