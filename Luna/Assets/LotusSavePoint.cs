using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class LotusSavePoint : MonoBehaviour
{
    [Header("Identity / Persistence")]
    [Tooltip("Must be unique for each lotus in the whole game, e.g. Level1_Lotus_0")]
    [SerializeField] private string lotusID = "Level1_Lotus_0";

    [Header("Save Settings")]
    [Tooltip("Optional custom respawn point. If left empty, this object's position will be used.")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("If true, this lotus will only save once per scene load.")]
    [SerializeField] private bool saveOnlyOnce = true;

    [Tooltip("Tag used to detect the player.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Load Protection")]
    [Tooltip("Prevents the lotus from instantly re-triggering when the scene loads and the player spawns inside it.")]
    [SerializeField] private float ignoreTriggerAfterSceneLoad = 0.5f;

    [Header("UI Pairing")]
    [Tooltip("Which UI lotus child to brighten when this world lotus is collected.")]
    [SerializeField] private int uiLotusIndex = 0;

    [Tooltip("Alpha used for dim/inactive UI lotuses.")]
    [Range(0f, 1f)]
    [SerializeField] private float dimUILotusAlpha = 0.5f;

    [Tooltip("Alpha used for bright/activated UI lotuses.")]
    [Range(0f, 1f)]
    [SerializeField] private float brightUILotusAlpha = 1f;

    [Header("UI Animation")]
    [SerializeField] private float uiLotusDelay = 0.2f;
    [SerializeField] private float uiLotusFadeDuration = 0.35f;

    [Header("UI Scene Visibility")]
    [Tooltip("Optional parent containing UI lotus icons. If left empty, the script will try to find one automatically.")]
    [SerializeField] private Transform uiLotusContainer;

    [Tooltip("Name of the UI lotus parent to search for if no container is assigned.")]
    [SerializeField] private string uiLotusContainerName = "UILotuses";

    [Tooltip("Scenes where the UI lotuses should be hidden.")]
    [SerializeField] private List<string> hideUIInScenes = new List<string> { "MainMenu", "MainMenuNoManager" };

    [Header("World Visuals")]
    [Tooltip("Usually the perched lotus sprite. If left empty, this object's SpriteRenderer will be used.")]
    [SerializeField] private SpriteRenderer perchedLotusRenderer;

    [Tooltip("Parent object that holds the lower ground lotuses. If left empty, the script will try to find a child named 'GroundLotuses'.")]
    [SerializeField] private GameObject groundLotusGroup;

    [Tooltip("Optional world-space Saved visual object. If left empty, the script will try to find a child named 'SavedTextVisual'.")]
    [SerializeField] private GameObject savedTextObject;

    [Header("Animation Timings")]
    [SerializeField] private float perchedFadeOutDuration = 0.35f;
    [SerializeField] private float groundFadeInDuration = 0.4f;
    [SerializeField] private float savedTextFadeInDuration = 0.15f;
    [SerializeField] private float savedTextVisibleDuration = 0.75f;
    [SerializeField] private float savedTextFadeOutDuration = 0.35f;

    [Header("Optional Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool hasSavedHere = false;
    private bool sequenceRunning = false;
    private bool collectedStateApplied = false;
    private float sceneLoadTime = -999f;

    private readonly List<SpriteRenderer> groundLotusRenderers = new List<SpriteRenderer>();
    private readonly List<SpriteRenderer> savedTextRenderers = new List<SpriteRenderer>();

    private const string HasSaveKey = "HasSave";
    private const string SavedSceneKey = "SavedScene";
    private const string SavedXKey = "SavedX";
    private const string SavedYKey = "SavedY";
    private const string SavedZKey = "SavedZ";

    private string CollectedKey => $"LotusCollected_{lotusID}";

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        sceneLoadTime = Time.time;

        if (perchedLotusRenderer == null)
        {
            perchedLotusRenderer = GetComponent<SpriteRenderer>();
        }

        AutoAssignMissingReferences();
        CacheRenderers();
        InitializeVisualStates();
        ApplyUISceneVisibility();
        RestoreCollectedStateIfNeeded();

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Awake on '{name}'. lotusID='{lotusID}'");
            Debug.Log($"[LotusSavePoint] Ground lotus renderers found: {groundLotusRenderers.Count}");
            Debug.Log($"[LotusSavePoint] Saved text renderers found: {savedTextRenderers.Count}");
            Debug.Log($"[LotusSavePoint] UI lotus container assigned: {uiLotusContainer != null}");
            Debug.Log($"[LotusSavePoint] UI lotus child count: {(uiLotusContainer != null ? uiLotusContainer.childCount : 0)}");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoadTime = Time.time;
        AutoAssignMissingReferences();
        ApplyUISceneVisibility();
        RestoreCollectedStateIfNeeded();
    }

    private void AutoAssignMissingReferences()
    {
        if (groundLotusGroup == null)
        {
            Transform foundGround = transform.Find("GroundLotuses");
            if (foundGround != null)
            {
                groundLotusGroup = foundGround.gameObject;
                if (debugLogs) Debug.Log($"[LotusSavePoint] Auto-assigned GroundLotuses on '{name}'.");
            }
        }

        if (savedTextObject == null)
        {
            Transform foundSavedText = transform.Find("SavedTextVisual");
            if (foundSavedText != null)
            {
                savedTextObject = foundSavedText.gameObject;
                if (debugLogs) Debug.Log($"[LotusSavePoint] Auto-assigned SavedTextVisual on '{name}'.");
            }
        }

        if (respawnPoint == null)
        {
            Transform foundRespawn = transform.Find("respawnPoint");
            if (foundRespawn != null)
            {
                respawnPoint = foundRespawn;
                if (debugLogs) Debug.Log($"[LotusSavePoint] Auto-assigned respawnPoint on '{name}'.");
            }
        }

        if (uiLotusContainer == null)
        {
            GameObject foundUIRoot = GameObject.Find(uiLotusContainerName);

            if (foundUIRoot != null)
            {
                Transform canvasChild = foundUIRoot.transform.Find("Canvas");
                uiLotusContainer = canvasChild != null ? canvasChild : foundUIRoot.transform;

                if (debugLogs)
                {
                    Debug.Log($"[LotusSavePoint] Auto-assigned UI lotus container to '{uiLotusContainer.name}'.");
                }
            }
            else if (debugLogs)
            {
                Debug.LogWarning($"[LotusSavePoint] Could not find UI lotus container named '{uiLotusContainerName}'.");
            }
        }
    }

    private void CacheRenderers()
    {
        groundLotusRenderers.Clear();
        savedTextRenderers.Clear();

        if (groundLotusGroup != null)
        {
            groundLotusRenderers.AddRange(groundLotusGroup.GetComponentsInChildren<SpriteRenderer>(true));
        }

        if (savedTextObject != null)
        {
            savedTextRenderers.AddRange(savedTextObject.GetComponentsInChildren<SpriteRenderer>(true));
        }
    }

    private void InitializeVisualStates()
    {
        if (groundLotusGroup != null)
        {
            groundLotusGroup.SetActive(true);
        }

        SetRendererGroupAlpha(groundLotusRenderers, 0f);
        SetRendererGroupEnabled(groundLotusRenderers, false);

        if (savedTextObject != null)
        {
            savedTextObject.SetActive(true);
        }

        SetRendererGroupAlpha(savedTextRenderers, 0f);
        SetRendererGroupEnabled(savedTextRenderers, false);
    }

    private void ApplyUISceneVisibility()
    {
        if (uiLotusContainer == null)
            return;

        string currentSceneName = SceneManager.GetActiveScene().name;
        bool shouldHide = hideUIInScenes.Contains(currentSceneName);

        Canvas canvas = uiLotusContainer.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiLotusContainer.GetComponentInParent<Canvas>(true);
        }

        if (canvas != null)
        {
            canvas.enabled = !shouldHide;

            if (debugLogs)
            {
                Debug.Log($"[LotusSavePoint] UI lotus canvas {(shouldHide ? "hidden" : "shown")} in scene '{currentSceneName}'.");
            }
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[LotusSavePoint] No Canvas found for UI lotuses.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (Time.time - sceneLoadTime < ignoreTriggerAfterSceneLoad)
        {
            if (debugLogs) Debug.Log("[LotusSavePoint] Ignored trigger because scene just loaded.");
            return;
        }

        if (sequenceRunning)
            return;

        if (IsCollected())
            return;

        if (saveOnlyOnce && hasSavedHere)
            return;

        SaveAtThisLotus();
        MarkCollected();
        hasSavedHere = true;
        StartCoroutine(PlaySaveSequence());
    }

    public void SaveAtThisLotus()
    {
        Vector3 savePosition = respawnPoint != null ? respawnPoint.position : transform.position;
        string sceneName = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(SavedSceneKey, sceneName);
        PlayerPrefs.SetFloat(SavedXKey, savePosition.x);
        PlayerPrefs.SetFloat(SavedYKey, savePosition.y);
        PlayerPrefs.SetFloat(SavedZKey, savePosition.z);
        PlayerPrefs.Save();

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Saved scene '{sceneName}' at position {savePosition}");
        }
    }

    private void MarkCollected()
    {
        if (string.IsNullOrWhiteSpace(lotusID))
        {
            Debug.LogWarning($"[LotusSavePoint] '{name}' has no lotusID. Collected state cannot persist.");
            return;
        }

        PlayerPrefs.SetInt(CollectedKey, 1);
        PlayerPrefs.Save();

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Marked lotus '{lotusID}' as collected.");
        }
    }

    private bool IsCollected()
    {
        if (string.IsNullOrWhiteSpace(lotusID))
            return false;

        return PlayerPrefs.GetInt(CollectedKey, 0) == 1;
    }

    private void RestoreCollectedStateIfNeeded()
    {
        if (collectedStateApplied)
            return;

        if (!IsCollected())
            return;

        ApplyCollectedStateInstantly();
        collectedStateApplied = true;
        hasSavedHere = true;

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Restored collected state for '{lotusID}'.");
        }
    }

    private void ApplyCollectedStateInstantly()
    {
        if (perchedLotusRenderer != null)
        {
            Color c = perchedLotusRenderer.color;
            c.a = 0f;
            perchedLotusRenderer.color = c;
            perchedLotusRenderer.enabled = false;
        }

        if (groundLotusRenderers.Count > 0)
        {
            SetRendererGroupEnabled(groundLotusRenderers, true);
            SetRendererGroupAlpha(groundLotusRenderers, 1f);
        }

        BrightenSpecificUILotusInstant(uiLotusIndex);

        if (savedTextRenderers.Count > 0)
        {
            SetRendererGroupAlpha(savedTextRenderers, 0f);
            SetRendererGroupEnabled(savedTextRenderers, false);
        }
    }

    private IEnumerator PlaySaveSequence()
    {
        sequenceRunning = true;

        BrightenSpecificUILotusInstant(uiLotusIndex);

        if (perchedLotusRenderer != null)
        {
            StartCoroutine(FadeSingleRenderer(
                perchedLotusRenderer,
                perchedLotusRenderer.color.a,
                0f,
                perchedFadeOutDuration,
                true));
        }

        if (groundLotusRenderers.Count > 0)
        {
            if (debugLogs)
            {
                Debug.Log($"[LotusSavePoint] Turning on {groundLotusRenderers.Count} ground lotus renderers.");
            }

            SetRendererGroupEnabled(groundLotusRenderers, true);
            StartCoroutine(FadeRendererGroup(groundLotusRenderers, 0f, 1f, groundFadeInDuration, false));
        }
        else if (debugLogs)
        {
            Debug.LogWarning("[LotusSavePoint] No ground lotus renderers found to turn on.");
        }

        if (savedTextRenderers.Count > 0)
        {
            yield return StartCoroutine(PlaySavedTextSequence());
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(perchedFadeOutDuration, groundFadeInDuration));
        }

        sequenceRunning = false;
    }

    private IEnumerator PlaySavedTextSequence()
    {
        SetRendererGroupEnabled(savedTextRenderers, true);
        yield return StartCoroutine(FadeRendererGroup(savedTextRenderers, 0f, 1f, savedTextFadeInDuration, false));
        yield return new WaitForSeconds(savedTextVisibleDuration);
        yield return StartCoroutine(FadeRendererGroup(savedTextRenderers, 1f, 0f, savedTextFadeOutDuration, true));
    }

    private IEnumerator BrightenSpecificUILotusDelayed(int index)
    {
        if (uiLotusDelay > 0f)
        {
            yield return new WaitForSeconds(uiLotusDelay);
        }

        yield return StartCoroutine(FadeSpecificUILotus(index));
    }

    private IEnumerator FadeSpecificUILotus(int index)
    {
        if (!TryGetUILotusImages(index, out List<Image> images, out string panelName))
        {
            yield break;
        }

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Fading UI lotus index {index} on '{panelName}'. Image count = {images.Count}");
        }

        if (uiLotusFadeDuration <= 0f)
        {
            SetImageGroupAlpha(images, brightUILotusAlpha);
            yield break;
        }

        List<float> startAlphas = new List<float>(images.Count);
        for (int i = 0; i < images.Count; i++)
        {
            startAlphas.Add(images[i] != null ? images[i].color.a : dimUILotusAlpha);
        }

        float elapsed = 0f;

        while (elapsed < uiLotusFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / uiLotusFadeDuration);

            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] == null) continue;

                Color c = images[i].color;
                c.a = Mathf.Lerp(startAlphas[i], brightUILotusAlpha, t);
                images[i].color = c;
            }

            yield return null;
        }

        SetImageGroupAlpha(images, brightUILotusAlpha);

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] UI lotus fade complete for '{panelName}'.");
        }
    }

    private void BrightenSpecificUILotusInstant(int index)
    {
        if (!TryGetUILotusImages(index, out List<Image> images, out _))
            return;

        SetImageGroupAlpha(images, brightUILotusAlpha);
    }

    public void DimAllUILotuses()
    {
        if (uiLotusContainer == null)
            return;

        for (int i = 0; i < uiLotusContainer.childCount; i++)
        {
            if (TryGetUILotusImages(i, out List<Image> images, out _))
            {
                SetImageGroupAlpha(images, dimUILotusAlpha);
            }
        }

        if (debugLogs)
        {
            Debug.Log("[LotusSavePoint] Dimmed all UI lotuses.");
        }
    }

    private bool TryGetUILotusImages(int index, out List<Image> images, out string panelName)
    {
        images = new List<Image>();
        panelName = "";

        if (uiLotusContainer == null)
        {
            if (debugLogs) Debug.LogWarning("[LotusSavePoint] UI lotus container is null.");
            return false;
        }

        if (index < 0 || index >= uiLotusContainer.childCount)
        {
            if (debugLogs) Debug.LogWarning($"[LotusSavePoint] UI lotus index {index} is out of range. Child count = {uiLotusContainer.childCount}");
            return false;
        }

        Transform child = uiLotusContainer.GetChild(index);
        panelName = child.name;

        Image[] found = child.GetComponentsInChildren<Image>(true);
        if (found == null || found.Length == 0)
        {
            if (debugLogs) Debug.LogWarning($"[LotusSavePoint] No Images found under UI lotus child '{child.name}'.");
            return false;
        }

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null)
            {
                images.Add(found[i]);
            }
        }

        return images.Count > 0;
    }

    private void SetImageGroupAlpha(List<Image> images, float alpha)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] == null) continue;

            Color c = images[i].color;
            c.a = alpha;
            images[i].color = c;
        }
    }

    private IEnumerator FadeSingleRenderer(SpriteRenderer renderer, float startAlpha, float endAlpha, float duration, bool disableAtEnd)
    {
        if (renderer == null)
            yield break;

        renderer.enabled = true;

        Color color = renderer.color;
        color.a = startAlpha;
        renderer.color = color;

        if (duration <= 0f)
        {
            color.a = endAlpha;
            renderer.color = color;

            if (disableAtEnd && Mathf.Approximately(endAlpha, 0f))
            {
                renderer.enabled = false;
            }

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            renderer.color = color;

            yield return null;
        }

        color.a = endAlpha;
        renderer.color = color;

        if (disableAtEnd && Mathf.Approximately(endAlpha, 0f))
        {
            renderer.enabled = false;
        }
    }

    private IEnumerator FadeRendererGroup(List<SpriteRenderer> renderers, float startAlpha, float endAlpha, float duration, bool disableAtEnd)
    {
        if (renderers == null || renderers.Count == 0)
            yield break;

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
                Color c = renderers[i].color;
                c.a = startAlpha;
                renderers[i].color = c;
            }
        }

        if (duration <= 0f)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    Color c = renderers[i].color;
                    c.a = endAlpha;
                    renderers[i].color = c;

                    if (disableAtEnd && Mathf.Approximately(endAlpha, 0f))
                    {
                        renderers[i].enabled = false;
                    }
                }
            }

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    Color c = renderers[i].color;
                    c.a = alpha;
                    renderers[i].color = c;
                }
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
            {
                Color c = renderers[i].color;
                c.a = endAlpha;
                renderers[i].color = c;

                if (disableAtEnd && Mathf.Approximately(endAlpha, 0f))
                {
                    renderers[i].enabled = false;
                }
            }
        }
    }

    private void SetRendererGroupAlpha(List<SpriteRenderer> renderers, float alpha)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] == null)
                continue;

            Color c = renderers[i].color;
            c.a = alpha;
            renderers[i].color = c;
        }
    }

    private void SetRendererGroupEnabled(List<SpriteRenderer> renderers, bool enabled)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = enabled;
            }
        }
    }

    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    public static string GetSavedScene(string fallbackScene = "")
    {
        return PlayerPrefs.GetString(SavedSceneKey, fallbackScene);
    }

    public static void TryLoadSavedPosition(Transform playerTransform, bool debugLogs = true)
    {
        if (playerTransform == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[LotusSavePoint] TryLoadSavedPosition failed: playerTransform is null.");
            }
            return;
        }

        if (PlayerPrefs.GetInt(HasSaveKey, 0) != 1)
        {
            if (debugLogs)
            {
                Debug.Log("[LotusSavePoint] No saved game found. Using default start position.");
            }
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string savedScene = PlayerPrefs.GetString(SavedSceneKey, "");

        if (savedScene != currentScene)
        {
            if (debugLogs)
            {
                Debug.Log($"[LotusSavePoint] Saved scene '{savedScene}' does not match current scene '{currentScene}'. Position not applied.");
            }
            return;
        }

        float x = PlayerPrefs.GetFloat(SavedXKey, playerTransform.position.x);
        float y = PlayerPrefs.GetFloat(SavedYKey, playerTransform.position.y);
        float z = PlayerPrefs.GetFloat(SavedZKey, playerTransform.position.z);

        playerTransform.position = new Vector3(x, y, z);

        if (debugLogs)
        {
            Debug.Log($"[LotusSavePoint] Loaded saved player position: {playerTransform.position}");
        }
    }

    public static void ClearSavedGame(bool debugLogs = true)
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(SavedSceneKey);
        PlayerPrefs.DeleteKey(SavedXKey);
        PlayerPrefs.DeleteKey(SavedYKey);
        PlayerPrefs.DeleteKey(SavedZKey);
        PlayerPrefs.Save();

        if (debugLogs)
        {
            Debug.Log("[LotusSavePoint] Cleared saved game data.");
        }
    }
}