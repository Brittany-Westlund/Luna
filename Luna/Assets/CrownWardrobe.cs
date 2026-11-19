using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CrownWardrobe : MonoBehaviour
{
    [Header("Luna Crown")]
    [Tooltip("Luna's existing MoonflowerCrownController.")]
    public MoonflowerCrownController crownController;

    [Tooltip("The SpriteRenderer that displays Luna's equipped crown.")]
    public SpriteRenderer equippedCrownRenderer;

    [Header("Wardrobe Crowns")]
    [Tooltip("Tag used by the crowns hanging in the environment.")]
    public string crownTag = "Crown";

    [Tooltip("How close Luna must be for a crown to become selectable.")]
    public float selectionRadius = 0.65f;

    [Tooltip("Key used to take, swap, or return crowns.")]
    public KeyCode swapKey = KeyCode.C;

    [Header("Selection Feedback")]
    [Tooltip("How much the nearby crown grows when selectable.")]
    public float selectedScaleMultiplier = 1.10f;

    [Tooltip("How quickly crowns grow/shrink.")]
    public float scaleSpeed = 10f;

    [Header("Equipped Crown Appearance")]
    [Tooltip("Additional scale applied to the crown while Luna wears it.")]
    public float equippedCrownScaleMultiplier = 1.5f;

    [Header("Debug")]
    public bool debugLogs = false;

    // Crown currently highlighted on the rack.
    private Transform _selectedCrown;
    private SpriteRenderer _selectedRenderer;
    private Vector3 _selectedOriginalScale;

    // Crown returning from highlighted scale.
    private Transform _returningCrown;
    private Vector3 _returningOriginalScale;

    // The physical rack crown Luna is currently wearing.
    private Transform _equippedRackCrown;
    private SpriteRenderer[] _equippedRackRenderers;

    // Original scale of Luna's wearable crown renderer.
    private Vector3 _equippedCrownBaseScale;

    private Coroutine _equipCoroutine;

    private void Awake()
    {
        if (crownController == null)
        {
            crownController =
                GetComponentInChildren<MoonflowerCrownController>(true);
        }

        if (equippedCrownRenderer == null &&
            crownController != null &&
            crownController.crownVisualRoot != null)
        {
            equippedCrownRenderer =
                crownController.crownVisualRoot
                    .GetComponentInChildren<SpriteRenderer>(true);
        }

        if (equippedCrownRenderer != null)
        {
            _equippedCrownBaseScale =
                equippedCrownRenderer.transform.localScale;
        }
    }

    private void Update()
    {
        FindNearestAvailableCrown();
        UpdateSelectionScale();
        UpdateReturningScale();

        if (Input.GetKeyDown(swapKey))
        {
            HandleCrownInput();
        }
    }

    // ---------------------------------------------------------
    // INPUT
    // ---------------------------------------------------------

    private void HandleCrownInput()
    {
        // There is another crown nearby:
        // take it, or swap our current crown for it.
        if (_selectedCrown != null)
        {
            EquipSelectedCrown();
            return;
        }

        // No other crown nearby.
        // If we're close to the empty position belonging to our
        // current crown, put it back.
        if (_equippedRackCrown != null)
        {
            float distance = Vector2.Distance(
                transform.position,
                _equippedRackCrown.position
            );

            if (distance <= selectionRadius)
            {
                ReturnCurrentCrown();
            }
        }
    }

    // ---------------------------------------------------------
    // FIND NEAREST CROWN
    // ---------------------------------------------------------

    private void FindNearestAvailableCrown()
    {
        GameObject[] crowns;

        try
        {
            crowns = GameObject.FindGameObjectsWithTag(crownTag);
        }
        catch (UnityException)
        {
            return;
        }

        Transform nearest = null;
        float nearestDistance = selectionRadius;

        foreach (GameObject crownObject in crowns)
        {
            if (crownObject == null)
            {
                continue;
            }

            Transform crown = crownObject.transform;

            // Don't select the crown Luna is already wearing.
            if (crown == _equippedRackCrown)
            {
                continue;
            }

            // Ignore crowns whose rack visuals are hidden.
            SpriteRenderer[] renderers =
                crown.GetComponentsInChildren<SpriteRenderer>(true);

            bool hasVisibleRenderer = false;

            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    hasVisibleRenderer = true;
                    break;
                }
            }

            if (!hasVisibleRenderer)
            {
                continue;
            }

            float distance = Vector2.Distance(
                transform.position,
                crown.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = crown;
            }
        }

        if (nearest == _selectedCrown)
        {
            return;
        }

        // Previous highlighted crown returns to normal size.
        if (_selectedCrown != null)
        {
            _returningCrown = _selectedCrown;
            _returningOriginalScale = _selectedOriginalScale;
        }

        _selectedCrown = nearest;
        _selectedRenderer = null;

        if (_selectedCrown != null)
        {
            _selectedOriginalScale = _selectedCrown.localScale;

            _selectedRenderer =
                _selectedCrown.GetComponentInChildren<SpriteRenderer>(true);

            if (debugLogs)
            {
                Debug.Log(
                    "[CrownWardrobe] Selected: " +
                    _selectedCrown.name
                );
            }
        }
    }

    // ---------------------------------------------------------
    // HIGHLIGHT
    // ---------------------------------------------------------

    private void UpdateSelectionScale()
    {
        if (_selectedCrown == null)
        {
            return;
        }

        Vector3 targetScale =
            _selectedOriginalScale * selectedScaleMultiplier;

        _selectedCrown.localScale = Vector3.Lerp(
            _selectedCrown.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

    private void UpdateReturningScale()
    {
        if (_returningCrown == null)
        {
            return;
        }

        _returningCrown.localScale = Vector3.Lerp(
            _returningCrown.localScale,
            _returningOriginalScale,
            Time.deltaTime * scaleSpeed
        );

        if (Vector3.Distance(
                _returningCrown.localScale,
                _returningOriginalScale) < 0.001f)
        {
            _returningCrown.localScale =
                _returningOriginalScale;

            _returningCrown = null;
        }
    }

    // ---------------------------------------------------------
    // EQUIP / SWAP
    // ---------------------------------------------------------

    private void EquipSelectedCrown()
{
    if (_selectedCrown == null ||
        _selectedRenderer == null ||
        equippedCrownRenderer == null)
    {
        return;
    }

    Transform newRackCrown = _selectedCrown;
    SpriteRenderer newRenderer = _selectedRenderer;
    Vector3 newOriginalScale = _selectedOriginalScale;

    // If Luna already has a crown, return that crown
    // to its rack position first.
    if (_equippedRackCrown != null)
    {
        ShowRackCrown(_equippedRackCrown);
    }

    // Make sure the selected crown is back at normal scale
    // before hiding it.
    newRackCrown.localScale = newOriginalScale;

    // Copy the wardrobe crown's sprite onto Luna.
    equippedCrownRenderer.sprite = newRenderer.sprite;

    // Copy the wardrobe crown's tint/hue too.
    // Keep it fully opaque while preserving its RGB values.
    Color wardrobeColor = newRenderer.color;
    wardrobeColor.a = 1f;
    equippedCrownRenderer.color = wardrobeColor;

    // Apply wearable crown scale.
    equippedCrownRenderer.transform.localScale =
        _equippedCrownBaseScale *
        equippedCrownScaleMultiplier;

    // Remember which physical crown Luna took.
    _equippedRackCrown = newRackCrown;

    _equippedRackRenderers =
        _equippedRackCrown.GetComponentsInChildren<SpriteRenderer>(true);

    // Hide the selected crown from the rack.
    HideRackCrown(_equippedRackCrown);

    // It's no longer an available wardrobe selection.
    _selectedCrown = null;
    _selectedRenderer = null;

    // The existing MoonflowerCrownController also sees C.
    // At the end of the frame, make sure the crown remains equipped.
    if (_equipCoroutine != null)
    {
        StopCoroutine(_equipCoroutine);
    }

    _equipCoroutine =
        StartCoroutine(EnsureCrownEquippedAtEndOfFrame());

    if (debugLogs)
    {
        Debug.Log(
            "[CrownWardrobe] Equipped: " +
            _equippedRackCrown.name
        );
    }
}

    // ---------------------------------------------------------
    // RETURN
    // ---------------------------------------------------------

    private void ReturnCurrentCrown()
    {
        if (_equippedRackCrown == null)
        {
            return;
        }

        if (debugLogs)
        {
            Debug.Log(
                "[CrownWardrobe] Returned: " +
                _equippedRackCrown.name
            );
        }

        ShowRackCrown(_equippedRackCrown);

        _equippedRackCrown = null;
        _equippedRackRenderers = null;

        if (_equipCoroutine != null)
        {
            StopCoroutine(_equipCoroutine);
        }

        // Existing controller also receives C this frame,
        // so force the final state OFF at end of frame.
        _equipCoroutine =
            StartCoroutine(EnsureCrownUnequippedAtEndOfFrame());
    }

    // ---------------------------------------------------------
    // RACK VISIBILITY
    // ---------------------------------------------------------

    private void HideRackCrown(Transform crown)
    {
        if (crown == null)
        {
            return;
        }

        SpriteRenderer[] renderers =
            crown.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    private void ShowRackCrown(Transform crown)
    {
        if (crown == null)
        {
            return;
        }

        SpriteRenderer[] renderers =
            crown.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    // ---------------------------------------------------------
    // EXISTING CROWN CONTROLLER COMPATIBILITY
    // ---------------------------------------------------------

    private IEnumerator EnsureCrownEquippedAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (crownController != null)
        {
            crownController.EnableCrown();
        }

        _equipCoroutine = null;
    }

    private IEnumerator EnsureCrownUnequippedAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (crownController != null)
        {
            crownController.DisableCrown();
        }

        _equipCoroutine = null;
    }

    // ---------------------------------------------------------
    // CLEANUP
    // ---------------------------------------------------------

    private void OnDisable()
    {
        if (_selectedCrown != null)
        {
            _selectedCrown.localScale =
                _selectedOriginalScale;
        }

        if (_returningCrown != null)
        {
            _returningCrown.localScale =
                _returningOriginalScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            selectionRadius
        );
    }
}