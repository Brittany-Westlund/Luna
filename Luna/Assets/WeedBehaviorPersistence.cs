using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class WeedBehaviorPersistent : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Reference to the CollectibleState asset that stores save data.")]
    public CollectibleState collectibleState;

    [Tooltip("A unique ID for this weed (e.g. 'Meadow_Weed_01').")]
    public string weedID;

    [Header("Detection Settings")]
    public float disappearRadius = 1.5f;

    [Header("Fade Settings")]
    public float fadeDelay = 0.5f;
    public float fadeDuration = 1f;

    private SpriteRenderer weedRenderer;
    private Collider2D weedCollider;
    private bool isFading = false;

    void Start()
    {
        weedRenderer = GetComponent<SpriteRenderer>();
        weedCollider = GetComponent<Collider2D>();

        // ✅ Make sure collider blocks movement
        weedCollider.isTrigger = false;

        // Auto-load state if not set
        if (collectibleState == null)
            collectibleState = Resources.Load<CollectibleState>("CollectibleState");

        // Already cleared? Hide + disable collision
        if (collectibleState != null && collectibleState.HasCollected(weedID))
        {
            weedRenderer.enabled = false;
            weedCollider.enabled = false;
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (isFading) return;

        // detect nearby grown grass
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, disappearRadius);
        foreach (var hit in hits)
        {
            var garden = hit.GetComponent<GardenGrowth>();
            if (garden != null && garden.grassObject != null && garden.grassObject.activeSelf)
            {
                StartCoroutine(FadeAndDisappear());
                break;
            }
        }
    }

    private IEnumerator FadeAndDisappear()
    {
        isFading = true;
        weedCollider.enabled = false; // ✨ stop blocking mid-fade for smoother movement

        if (fadeDelay > 0f)
            yield return new WaitForSeconds(fadeDelay);

        if (weedRenderer != null)
        {
            float elapsed = 0f;
            Color c = weedRenderer.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                weedRenderer.color = c;
                yield return null;
            }
        }

        // Mark weed cleared
        collectibleState?.MarkCollected(weedID);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.3f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, disappearRadius);
    }
}
