using UnityEngine;

public class FairyflowerVeil : MonoBehaviour
{
    public float requiredLight = 5f;       // Minimum light Luna must have
    public bool veilTooStrong = false;     // If true, this veil cannot be removed yet

    public float pulseDuration = 2f;       // Duration of pulse effect
    public float fadeDuration = 2f;        // Duration of dissolve/fade
    

    private SpriteRenderer sr;
    private bool playerInRange = false;
    private bool isProcessing = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerInRange && !isProcessing && Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(PulseThenCheck());
        }
    }

    System.Collections.IEnumerator PulseThenCheck()
    {
        isProcessing = true;

        // Start pulsing
        float timer = 0f;
        while (timer < pulseDuration)
        {
            float alpha = 0.6f + Mathf.Sin(Time.time * 8f) * 0.2f;  // Pulsing effect
            SetAlpha(alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        // Check light & strength
        LunaLightManager lunaLightManager = FindObjectOfType<LunaLightManager>();
        float lunaLight = lunaLightManager != null ? lunaLightManager.CurrentLight : 0f;

        if (lunaLight >= requiredLight && !veilTooStrong)
        {
            // Fade out veil
            float fadeTimer = 0f;
            while (fadeTimer < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
                SetAlpha(alpha);
                fadeTimer += Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);  // Fully gone
        }
        else
        {
            // Reset to original state
            SetAlpha(1f);
        }

        isProcessing = false;
    }

    void SetAlpha(float alpha)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
