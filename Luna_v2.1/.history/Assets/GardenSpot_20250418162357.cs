// GardenSpot.cs
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("The SpriteRenderer to tint for highlight (set its color darker in the Editor).")]
    public GameObject highlightObject;

    private SpriteRenderer _highlightRenderer;
    private Color          _originalColor;

    // Track the planted flower as before
    private GameObject plantedFlower;

    void Awake()
    {
        // Cache the SpriteRenderer and its original color
        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        // Register any pre‑placed child flower just like before
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }
    }

    /// <summary>
    /// Call to highlight (true) or un‑highlight (false) this spot.
    /// </summary>
    public void SetHighlight(bool on)
    {
        if (_highlightRenderer == null) return;

        // On highlight: tint white; otherwise restore original
        _highlightRenderer.color = on
            ? Color.white
            : _originalColor;
    }

    // ... rest of your methods remain unchanged:

    public void SetPlantedFlower(GameObject flower)   => plantedFlower = flower;
    public GameObject GetPlantedFlower()               => plantedFlower;
    public void ClearPlantedFlower()                   => plantedFlower = null;
    public Transform GetPlantingPoint()                => plantingPoint;
}
