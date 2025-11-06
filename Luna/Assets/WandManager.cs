using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)] // runs before most other scripts
public class WandManager : MonoBehaviour
{
    [Header("Wand Settings")]
    [Tooltip("Tag used to find the wand in the scene.")]
    public string wandTag = "Wand";

    [Tooltip("Collectible ID associated with the wand.")]
    public string wandID = "Wand01";

    private GameObject wandObject;

    private void Awake()
    {
        // make sure we persist across scenes
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(HandleSceneLoad());
    }

    private IEnumerator HandleSceneLoad()
    {
        // wait a frame for scene to fully initialize
        yield return null;

        // find wand by tag
        wandObject = GameObject.FindGameObjectWithTag(wandTag);

        if (wandObject == null)
        {
            Debug.LogWarning("[WandManager] No wand found in this scene.");
            yield break;
        }

        // always hide it immediately before anything else updates
        wandObject.SetActive(false);
        Debug.Log("[WandManager] Wand hidden on scene load.");

        // wait a tiny bit for CollectibleManager to finish loading
        yield return new WaitForSeconds(0.2f);

        bool hasWand = CollectibleManager.Instance != null &&
                       CollectibleManager.Instance.HasCollected(wandID);

        if (hasWand)
        {
            ShowWand(false);
        }
        else
        {
            HideWand();
        }
    }

    public void ShowWand(bool save = true)
    {
        if (wandObject == null)
        {
            wandObject = GameObject.FindGameObjectWithTag(wandTag);
            if (wandObject == null)
            {
                Debug.LogWarning("[WandManager] Tried to show wand, but it doesn't exist in the scene.");
                return;
            }
        }

        wandObject.SetActive(true);
        Debug.Log("[WandManager] Wand shown.");

        if (save && CollectibleManager.Instance != null)
            CollectibleManager.Instance.MarkCollected(wandID);
    }

    public void HideWand()
    {
        if (wandObject == null)
        {
            wandObject = GameObject.FindGameObjectWithTag(wandTag);
            if (wandObject == null)
            {
                Debug.LogWarning("[WandManager] Tried to hide wand, but it doesn't exist in the scene.");
                return;
            }
        }

        wandObject.SetActive(false);
        Debug.Log("[WandManager] Wand hidden.");
    }
}
