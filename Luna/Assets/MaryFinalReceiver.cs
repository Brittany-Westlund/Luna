using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaryFinalReceiver : MonoBehaviour
{
    [Header("Fade")]
    public float fadeSeconds = 1.0f;

    [Header("Main Visual Root")]
    public GameObject visualRoot; // leave empty if sprites are on Mary

    [Header("Optional Additional Fade Roots")]
    public GameObject[] additionalFadeRoots;

    [Header("Optional Objects To Disable At End")]
    public GameObject[] additionalObjectsToDisable;

    [Header("Optional")]
    public Collider2D interactionCollider;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool fading = false;
    private SpriteRenderer[] renderers;

    void Awake()
    {
        BuildRendererList();
    }

    void BuildRendererList()
    {
        List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();

        if (visualRoot == null)
            visualRoot = gameObject;

        if (visualRoot != null)
        {
            allRenderers.AddRange(visualRoot.GetComponentsInChildren<SpriteRenderer>(true));
        }

        if (additionalFadeRoots != null)
        {
            foreach (GameObject root in additionalFadeRoots)
            {
                if (root == null) continue;
                allRenderers.AddRange(root.GetComponentsInChildren<SpriteRenderer>(true));
            }
        }

        renderers = allRenderers.ToArray();
    }

    // Call this at the end of Mary's final conversation
    public void MarkConversationFinished()
    {
        if (fading) return;

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (debugLogs) Debug.Log("🌿 MaryFinalReceiver: convo finished, fading Mary and extras.");

        StartCoroutine(FadeThenDisable());
    }

    IEnumerator FadeThenDisable()
    {
        fading = true;

        if (renderers == null || renderers.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("🌿 MaryFinalReceiver: no SpriteRenderers found; disabling instantly.");
            DisableAllTargets();
            yield break;
        }

        Color[] start = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                start[i] = renderers[i].color;
        }

        float t = 0f;
        float dur = Mathf.Max(0.01f, fadeSeconds);

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float a = Mathf.Lerp(1f, 0f, k);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                Color c = start[i];
                c.a = a;
                renderers[i].color = c;
            }

            yield return null;
        }

        if (debugLogs) Debug.Log("🌿 MaryFinalReceiver: faded; disabling Mary and extras.");

        DisableAllTargets();
    }

    void DisableAllTargets()
    {
        if (gameObject != null)
            gameObject.SetActive(false);

        if (additionalObjectsToDisable != null)
        {
            foreach (GameObject obj in additionalObjectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}