using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using System.Collections;

public class LunaHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public MMProgressBar healthBar;
    public float decayInterval = 5f;
    public float healthLossAmount = 0.01f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private float nextDecayTime;
    private Health lunaHealth;
    private SpriteRenderer screenFader;

    private bool isSuppressed = false;
    private float suppressTimer = 0f;

    void Start()
    {
        nextDecayTime = Time.time + decayInterval;
        lunaHealth = GetComponent<Health>();

        GameObject fadeObject = new GameObject("ScreenFader");
        screenFader = fadeObject.AddComponent<SpriteRenderer>();
        screenFader.color = new Color(0, 0, 0, 0);
        screenFader.sortingOrder = 100;
        fadeObject.transform.position = Camera.main.transform.position + new Vector3(0, 0, 5);
        fadeObject.transform.localScale = new Vector3(30, 30, 1);
    }

    void Update()
    {
        if (isSuppressed)
        {
            suppressTimer -= Time.deltaTime;

            if (suppressTimer <= 0f)
            {
                isSuppressed = false;
                suppressTimer = 0f;
                Debug.Log("🕓 Anemone suppression expired — decay resumes.");
            }

            return; // skip decay while suppressed
        }

        if (Time.time >= nextDecayTime)
        {
            ApplyHealthDecay();
            nextDecayTime = Time.time + decayInterval;
        }

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

    private IEnumerator HandleDeath()
    {
        yield return StartCoroutine(FadeToBlack());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

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

    // 🌼 Called by Anemone pollen
    public void SuppressDecay(float duration)
    {
        isSuppressed = true;
        suppressTimer = duration;
        Debug.Log($"🛡️ Health decay suppressed for {duration} seconds.");
    }
}
