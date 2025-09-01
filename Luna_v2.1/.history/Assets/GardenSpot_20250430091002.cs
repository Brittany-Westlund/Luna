// GardenSpot.cs
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("SpriteRenderer object to tint for highlight.")]
    public GameObject highlightObject;

    private SpriteRenderer _highlightRenderer;
    private Color          _originalColor;

    // Which flower is planted here
    private GameObject plantedFlower;

    void Awake()
    {
        // Cache the SpriteRenderer & its original color
        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        // Auto‑register any pre‑placed child flower
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null &&
                Vector2.Distance(c.transform.position, GetPlantingPoint().position) < 0.1f)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }

        /*for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }*/
    }

    /// <summary>Called every frame by the manager.</summary>
    public void SetHighlight(bool on)
    {
        if (_highlightRenderer == null) return;
        _highlightRenderer.color = on ? Color.white : _originalColor;
    }

    public void SetPlantedFlower(GameObject flower)
    {
        plantedFlower = flower;
    }

    public GameObject GetPlantedFlower()
    {
        return plantedFlower;
    }

    public void ClearPlantedFlower()
    {
        plantedFlower = null;
    }

    public Transform GetPlantingPoint()
    {
        return plantingPoint;
    }
}
