using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LotusUIFadeIn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float delayBeforeFade = 0.3f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Hidden Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainMenuNoManagerSceneName = "MainMenuNoManager";

    [Header("Options")]
    [SerializeField] private bool fadeOnSceneLoad = true;
    [SerializeField] private bool hideInstantlyInBlockedScenes = true;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            Debug.LogWarning("[LotusUIFadeIn] No CanvasGroup found.");
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HandleScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!fadeOnSceneLoad)
            return;

        HandleScene(scene.name);
    }

    private void HandleScene(string sceneName)
    {
        if (canvasGroup == null)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        bool shouldHide =
            sceneName == mainMenuSceneName ||
            sceneName == mainMenuNoManagerSceneName;

        if (shouldHide)
        {
            if (hideInstantlyInBlockedScenes)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        fadeCoroutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        if (delayBeforeFade > 0f)
            yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        fadeCoroutine = null;
    }
}