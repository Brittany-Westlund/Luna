using UnityEngine;
using UnityEngine.UI;

public class ToggleSpriteOnEInteraction : MonoBehaviour
{
    [Header("Interact Prompt UI")]
    [SerializeField] private Sprite interactPromptSprite;
    [SerializeField] private Vector3 promptWorldOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private float promptFadeSpeed = 8f;

    [Header("Prompt Scale")]
    [Range(0.1f, 5f)]
    [SerializeField] private float promptScale = 1f;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactCooldown = 0.15f;

    [Header("Prompt Behavior")]
    [SerializeField] private bool showPromptOnlyOnce = false;

    private Image interactPromptImage;
    private RectTransform interactPromptRect;
    private Canvas promptCanvas;

    private SpriteRenderer sittingSpriteRenderer;
    private SpriteRenderer lunaMainSpriteRenderer;
    private Animator lunaMainAnimator;

    private bool playerInRange = false;
    private bool showPrompt = false;
    private float promptAlpha = 0f;

    private bool isSitting = false;
    private float nextInteractTime = 0f;
    private bool promptHasBeenUsed = false;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        sittingSpriteRenderer = GetComponent<SpriteRenderer>();
        if (sittingSpriteRenderer != null)
        {
            sittingSpriteRenderer.enabled = false;
        }

        CreatePromptUI();
        SetPromptAlpha(0f);
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        UpdatePromptPosition();
        UpdatePromptScale();
        HandlePromptFade();

        if (isSitting)
        {
            ForceSittingVisualState();
        }

        if (isSitting && PlayerIsMoving())
        {
            ExitSittingState();
            showPrompt = playerInRange && !(showPromptOnlyOnce && promptHasBeenUsed);
            return;
        }

        if (!playerInRange) return;
        if (Time.time < nextInteractTime) return;

        if (Input.GetKeyDown(interactKey))
        {
            nextInteractTime = Time.time + interactCooldown;
            promptHasBeenUsed = true;

            if (isSitting)
            {
                ExitSittingState();
                showPrompt = !showPromptOnlyOnce;
            }
            else
            {
                EnterSittingState();
                showPrompt = false;
            }
        }
    }

    private void CreatePromptUI()
    {
        GameObject canvasObject = new GameObject("InteractPromptCanvas");
        promptCanvas = canvasObject.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptCanvas.sortingOrder = 500;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("InteractPromptImage");
        imageObject.transform.SetParent(canvasObject.transform, false);

        interactPromptImage = imageObject.AddComponent<Image>();
        interactPromptImage.sprite = interactPromptSprite;
        interactPromptImage.preserveAspect = true;
        interactPromptImage.raycastTarget = false;

        interactPromptRect = interactPromptImage.GetComponent<RectTransform>();
        interactPromptRect.anchorMin = new Vector2(0.5f, 0.5f);
        interactPromptRect.anchorMax = new Vector2(0.5f, 0.5f);
        interactPromptRect.pivot = new Vector2(0.5f, 0.5f);
        interactPromptRect.sizeDelta = new Vector2(64f, 64f);
        interactPromptRect.localScale = Vector3.one * promptScale;
    }

    private void UpdatePromptPosition()
    {
        if (interactPromptRect == null || mainCamera == null) return;

        Vector3 worldPos = transform.position + promptWorldOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        bool behindCamera = screenPos.z < 0f;
        interactPromptRect.gameObject.SetActive(!behindCamera);

        if (!behindCamera)
        {
            interactPromptRect.position = screenPos;
        }
    }

    private void UpdatePromptScale()
    {
        if (interactPromptRect == null) return;

        interactPromptRect.localScale = Vector3.one * promptScale;
    }

    private void HandlePromptFade()
    {
        if (interactPromptImage == null) return;

        float target = showPrompt ? 1f : 0f;
        promptAlpha = Mathf.MoveTowards(promptAlpha, target, promptFadeSpeed * Time.deltaTime);
        SetPromptAlpha(promptAlpha);
    }

    private void SetPromptAlpha(float alpha)
    {
        if (interactPromptImage == null) return;

        Color c = interactPromptImage.color;
        c.a = alpha;
        interactPromptImage.color = c;
    }

    private void EnterSittingState()
    {
        isSitting = true;
        ForceSittingVisualState();
    }

    private void ExitSittingState()
    {
        isSitting = false;
        ForceNormalVisualState();
    }

    private void ForceSittingVisualState()
    {
        if (sittingSpriteRenderer != null)
        {
            sittingSpriteRenderer.enabled = true;
        }

        if (lunaMainSpriteRenderer != null)
        {
            lunaMainSpriteRenderer.enabled = false;
        }

        if (lunaMainAnimator != null)
        {
            lunaMainAnimator.enabled = false;
        }
    }

    private void ForceNormalVisualState()
    {
        if (sittingSpriteRenderer != null)
        {
            sittingSpriteRenderer.enabled = false;
        }

        if (lunaMainSpriteRenderer != null)
        {
            lunaMainSpriteRenderer.enabled = true;
        }

        if (lunaMainAnimator != null)
        {
            lunaMainAnimator.enabled = true;
            lunaMainAnimator.Rebind();
            lunaMainAnimator.Update(0f);
        }
    }

    private bool PlayerIsMoving()
    {
        if (Input.GetAxisRaw("Horizontal") != 0f) return true;
        if (Input.GetAxisRaw("Vertical") != 0f) return true;

        if (Input.GetKey(KeyCode.LeftArrow)) return true;
        if (Input.GetKey(KeyCode.RightArrow)) return true;
        if (Input.GetKey(KeyCode.UpArrow)) return true;
        if (Input.GetKey(KeyCode.DownArrow)) return true;

        if (Input.GetKey(KeyCode.A)) return true;
        if (Input.GetKey(KeyCode.D)) return true;
        if (Input.GetKey(KeyCode.W)) return true;
        if (Input.GetKey(KeyCode.S)) return true;

        return false;
    }

    private void CachePlayerVisualReferences(Collider2D other)
    {
        if (other == null) return;

        if (lunaMainAnimator == null)
        {
            Animator[] animators = other.GetComponentsInChildren<Animator>(true);

            foreach (Animator anim in animators)
            {
                if (anim == null) continue;

                // Prefer enabled or likely-main animators first.
                if (anim.runtimeAnimatorController != null)
                {
                    lunaMainAnimator = anim;
                    break;
                }
            }

            if (lunaMainAnimator == null && animators.Length > 0)
            {
                lunaMainAnimator = animators[0];
            }
        }

        if (lunaMainSpriteRenderer == null)
        {
            // Best case: renderer is on same object as the animator.
            if (lunaMainAnimator != null)
            {
                lunaMainSpriteRenderer = lunaMainAnimator.GetComponent<SpriteRenderer>();

                // If not on same object, look under animator object first.
                if (lunaMainSpriteRenderer == null)
                {
                    lunaMainSpriteRenderer = lunaMainAnimator.GetComponentInChildren<SpriteRenderer>(true);
                }

                // Then check parent of animator.
                if (lunaMainSpriteRenderer == null && lunaMainAnimator.transform.parent != null)
                {
                    lunaMainSpriteRenderer = lunaMainAnimator.transform.parent.GetComponent<SpriteRenderer>();
                }
            }

            // Last resort fallback.
            if (lunaMainSpriteRenderer == null)
            {
                SpriteRenderer[] renderers = other.GetComponentsInChildren<SpriteRenderer>(true);

                foreach (SpriteRenderer sr in renderers)
                {
                    if (sr == null) continue;
                    if (sr == sittingSpriteRenderer) continue;

                    lunaMainSpriteRenderer = sr;
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        showPrompt = !isSitting && !(showPromptOnlyOnce && promptHasBeenUsed);

        CachePlayerVisualReferences(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        showPrompt = false;

        ExitSittingState();
    }

    private void OnDestroy()
    {
        if (promptCanvas != null)
        {
            Destroy(promptCanvas.gameObject);
        }
    }
}