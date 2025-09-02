using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTextManager : MonoBehaviour
{
    public Text levelText;
    public float hideTimer = 10f; // public variable for hide timer
    public float fadeDuration = 3f; // public variable for fade duration

    private bool isGamePaused = false;
    
    // Call this method to start the hide timer
    public void StartHideTimer()
    {
        Invoke(nameof(StartFadeOut), hideTimer);
    }

    // Call this method to start fading out the text
    private void StartFadeOut()
    {
        if (!isGamePaused)
        {
            StartCoroutine(FadeOutText());
        }
    }

    // Coroutine to fade out the text
    private IEnumerator FadeOutText()
    {
        float elapsedTime = 0;
        Color originalColor = levelText.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            levelText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        levelText.enabled = false; // Disable the text after fading
        levelText.color = originalColor; // Reset color to original (optional)
    }

    // Call this method when the game is paused
    public void OnGamePaused()
    {
        isGamePaused = true;
        CancelInvoke(nameof(StartFadeOut));
        levelText.enabled = true; // Make the text visible again
        StopCoroutine(FadeOutText()); // Stop the fade out if it's happening
        levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 1f); // Reset alpha to full
    }

    // Call this method when the game is resumed
    public void OnGameResumed()
    {
        isGamePaused = false;
        StartHideTimer(); // Start the timer to hide the text again
    }

    // Initialize
    void Start()
    {
        StartHideTimer();
    }
}
