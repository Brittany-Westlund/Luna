using UnityEngine;
using System.Collections;
using TMPro;

public class LunaProximityPrompt : MonoBehaviour
{
    [Header("Detection Settings")]
    public string sporeHintTag = "SporeIcon"; // Tag on your spore hint
    public float fadeSpeed = 4f;              // How fast prompt fades in/out

    [Header("Prompt Settings")]
    public GameObject promptObject;           // Child prompt to show/hide
    private CanvasGroup canvasGroup;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMesh;

    private bool isNear = false;
    private Coroutine fadeRoutine;

    void Start()
    {
        if (promptObject == null)
        {
            Debug.LogWarning("[LunaProximityPrompt] No promptObject assigned!");
            return;
        }

        // Try to get common renderer types
        canvasGroup = promptObject.GetComponent<CanvasGroup>();
        spriteRenderer = promptObject.GetComponent<SpriteRenderer>();
        textMesh = promptObject.GetComponent<TextMeshPro>();

        // Start hidden
        SetAlpha(0f);
        promptObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(sporeHintTag)) return;

        isNear = true;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadePrompt(1f)); // fade in
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(sporeHintTag)) return;

        isNear = false;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadePrompt(0f)); // fade out
    }

    private IEnumerator FadePrompt(float target)
    {
        float start = GetAlpha();
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float a = Mathf.Lerp(start, target, t);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(target);
    }

    private void SetAlpha(float a)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = a;
        }
        else if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = a;
            spriteRenderer.color = c;
        }
        else if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = a;
            textMesh.color = c;
        }
    }

    private float GetAlpha()
    {
        if (canvasGroup != null) return canvasGroup.alpha;
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (textMesh != null) return textMesh.color.a;
        return 1f;
    }
}
