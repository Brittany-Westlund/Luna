using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LunaHealthManager : MonoBehaviour
{
    [Header("Auto-Linked References")]
    [SerializeField] private Image healthBarFill; // assign manually only if you want to override

    private bool triedFinding = false;

    void Awake()
    {
        // Try to find immediately (in most scenes HUD is loaded at start)
        TryFindHealthBar();
    }

    IEnumerator Start()
    {
        // If not found yet (e.g., HUD loads a frame later), keep checking briefly
        if (healthBarFill == null)
        {
            yield return new WaitForSeconds(0.1f);
            TryFindHealthBar();
        }
    }

    private void TryFindHealthBar()
    {
        if (healthBarFill != null || triedFinding) return;

        triedFinding = true;

        // Look for HUD under Canvas or UICamera
        GameObject hud = GameObject.Find("HUD");

        if (hud == null)
        {
            // broader fallback
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform hudTransform = canvas.transform.Find("HUD");
                if (hudTransform != null)
                    hud = hudTransform.gameObject;
            }
        }

        if (hud != null)
        {
            Transform healthBar = hud.transform.Find("HealthBar");
            if (healthBar != null)
            {
                // try to grab an Image from HealthBar or its children
                healthBarFill = healthBar.GetComponentInChildren<Image>();
                Debug.Log($"[AutoFindHealthBar] Found HealthBar: {healthBarFill?.name}");
            }
            else
            {
                Debug.LogWarning("[AutoFindHealthBar] Could not find HealthBar under HUD.");
            }
        }
        else
        {
            Debug.LogWarning("[AutoFindHealthBar] Could not find HUD in scene.");
        }
    }

    /// <summary>
    /// Call this whenever you need to update health.
    /// 'value' should be a normalized 0–1 float.
    /// </summary>
    public void SetHealth(float value)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Clamp01(value);
        }
        else
        {
            Debug.LogWarning("[AutoFindHealthBar] HealthBarFill missing—cannot update health.");
        }
    }

    // 🌼 Called by Anemone pollen
    public void SuppressDecay(float duration)
    {
        Debug.Log($"🛡️ SuppressDecay({duration}) called — no decay logic in this version.");
    }

}


/* using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
using System.Collections;

public class LunaHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public MMProgressBar healthBar;          // optional now
    public float decayInterval = 5f;
    public float healthLossAmount = 0.01f;   // fraction of max per interval

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public Color fadeColor = Color.black;    // lets you tint if desired

    private float _nextDecayTime;
    private Health _lunaHealth;

    // Screen fader
    private static GameObject _fadeGO;             // persist across scenes
    private static SpriteRenderer _screenFader;    // persist across scenes

    private bool _isSuppressed = false;
    private float _suppressTimer = 0f;
    private bool _isDying = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedRepositionFader;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedRepositionFader;
    }

    void Start()
    {
        _nextDecayTime = Time.time + decayInterval;
        _lunaHealth = GetComponent<Health>();

        EnsureFaderExists();
        RepositionFaderToCamera(Camera.main);
        SetFaderAlpha(0f); // start fully transparent
    }

    void Update()
    {
        if (_isDying) return; // stop decay while dying

        if (_isSuppressed)
        {
            _suppressTimer -= Time.deltaTime;
            if (_suppressTimer <= 0f)
            {
                _isSuppressed = false;
                _suppressTimer = 0f;
                Debug.Log("🕓 Anemone suppression expired — decay resumes.");
            }
            return;
        }

        if (Time.time >= _nextDecayTime)
        {
            ApplyHealthDecay();
            _nextDecayTime = Time.time + decayInterval;
        }

        if (_lunaHealth != null && _lunaHealth.CurrentHealth <= 0 && !_isDying)
        {
            StartCoroutine(HandleDeath());
        }
    }

    private void ApplyHealthDecay()
    {
        if (_lunaHealth == null) return;

        float newHealth = Mathf.Max(0f, _lunaHealth.CurrentHealth - healthLossAmount * _lunaHealth.MaximumHealth);
        _lunaHealth.SetHealth(newHealth, gameObject);

        // Optional bar update if assigned
        if (healthBar != null && _lunaHealth.MaximumHealth > 0f)
        {
            healthBar.SetBar01(newHealth / _lunaHealth.MaximumHealth);
        }
    }

    private IEnumerator HandleDeath()
    {
        _isDying = true;

        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Lose a life (Corgi flow)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }

        // Reload scene
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        while (!op.isDone) { yield return null; } // wait load

        // After scene loaded, the fader persists but camera changed → reposition in handler
        // Give one frame for Camera.main to be valid if needed
        yield return null;

        // Fade from black
        yield return StartCoroutine(FadeFromBlack());

        _isDying = false;
    }

    // === Fader Management ===

    private void EnsureFaderExists()
    {
        if (_screenFader != null) return;

        // Create once and persist
        _fadeGO = new GameObject("ScreenFader_Persistent");
        _screenFader = _fadeGO.AddComponent<SpriteRenderer>();
        _screenFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        _screenFader.sortingOrder = 32767; // very top

        DontDestroyOnLoad(_fadeGO);

        // Make it large enough for typical ortho cams; we’ll reposition/scale each scene.
        _fadeGO.transform.localScale = new Vector3(40f, 40f, 1f);
    }

    private void OnSceneLoadedRepositionFader(Scene scene, LoadSceneMode mode)
    {
        // Each new scene: move fader in front of new main camera
        RepositionFaderToCamera(Camera.main);
    }

    private void RepositionFaderToCamera(Camera cam)
    {
        if (_screenFader == null) return;

        if (cam == null)
        {
            // try again next frame if camera not ready yet
            StartCoroutine(WaitAndReposition());
            return;
        }

        // Place slightly in front of camera
        Vector3 pos = cam.transform.position + cam.transform.forward * 5f;
        // For 2D orthographic, forward is typically (0,0,-1), so we push "in front" relative to camera

        _screenFader.transform.position = pos;

        // Auto scale for orthographic cameras (covers viewport)
        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            // Convert to sprite units (assuming default 1 unit = 1 world unit rectangle)
            _screenFader.transform.localScale = new Vector3(width * 1.1f, height * 1.1f, 1f);
        }
    }

    private IEnumerator WaitAndReposition()
    {
        yield return null;
        RepositionFaderToCamera(Camera.main);
    }

    private void SetFaderAlpha(float a)
    {
        if (_screenFader == null) return;
        var c = _screenFader.color;
        _screenFader.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
    }

    private IEnumerator FadeToBlack()
    {
        EnsureFaderExists();
        RepositionFaderToCamera(Camera.main);

        float t = 0f;
        while (t < fadeDuration)
        {
            if (_screenFader == null) yield break; // destroyed somehow
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            _screenFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        if (_screenFader != null)
            _screenFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
    }

    private IEnumerator FadeFromBlack()
    {
        EnsureFaderExists();
        RepositionFaderToCamera(Camera.main);

        float t = 0f;
        while (t < fadeDuration)
        {
            if (_screenFader == null) yield break;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            _screenFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        if (_screenFader != null)
            _screenFader.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }

    // 🌼 Called by Anemone pollen
    public void SuppressDecay(float duration)
    {
        _isSuppressed = true;
        _suppressTimer = duration;
        Debug.Log($"🛡️ Health decay suppressed for {duration} seconds.");
    }
}


/ using UnityEngine;
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
*/
