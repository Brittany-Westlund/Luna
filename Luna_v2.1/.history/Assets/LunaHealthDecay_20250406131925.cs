using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using System.Collections;

public class LunaHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public MMProgressBar healthBar;      // Reference to Luna's health bar
    public float decayInterval = 5f;     // Time between each health decay
    public float healthLossAmount = 0.01f; // Amount of health lost per interval

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;    // Time for fade-to-black effect

    private float nextDecayTime;
    private Health lunaHealth;
    private SpriteRenderer screenFader; // Uses a SpriteRenderer for fade effect

    private bool isSuppressed = false;
    private float suppressTimer = 0f;

    void Start()
    {
        nextDecayTime = Time.time + decayInterval;
        lunaHealth = GetComponent<Health>();

        // Create the fade effect without UI
        GameObject fadeObject = new GameObject("ScreenFader");
        screenFader = fadeObject.AddComponent<SpriteRenderer>();
        screenFader.color = new Color(0, 0, 0, 0); // Fully transparent
        screenFader.sortingOrder = 100; // Ensure it's on top
        fadeObject.transform.position = Camera.main.transform.position + new Vector3(0, 0, 5);
        fadeObject.transform.localScale = new Vector3(30, 30, 1); // Covers screen
    }

    void Update()
    {
        if (isSuppressed)
    {
        suppressTimer -= Time.deltaTime;
        if (suppressTimer <= 0)
        {
            isSuppressed = false;
        }
        return; // Skip decay logic while suppressed
    }
       
        if (Time.time >= nextDecayTime)
        {
            ApplyHealthDecay();
            nextDecayTime = Time.time + decayInterval;
        }

        // Check for death
        if (lunaHealth.CurrentHealth <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }

    private void ApplyHealthDecay()
    {
        float newHealth = Mathf.Max(0, lunaHealth.CurrentHealth - healthLossAmount * lunaHealth.MaximumHealth);
        lunaHealth.SetHealth(newHealth, gameObject);
        healthBar.SetBar01(newHealth / lunaHealth.MaximumHealth);
    }
    public void SuppressDecay(float duration)
    {
        isSuppressed = true;
        suppressTimer = duration;
    }
    private IEnumerator HandleDeath()
    {
        // Start fade to black
        yield return StartCoroutine(FadeToBlack());

        // Deduct a life if GameManager exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }

        // Restart scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Fade back in
        yield return StartCoroutine(FadeFromBlack());
    }

    private IEnumerator FadeToBlack()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            screenFader.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        screenFader.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator FadeFromBlack()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            screenFader.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        screenFader.color = new Color(0, 0, 0, 0);
    }

    
}
