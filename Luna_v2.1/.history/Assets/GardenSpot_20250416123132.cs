using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [SerializeField] private Transform plantingPoint;
    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;

    private void Start()
    {
        grassRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public bool HasPlantedFlower => plantedFlower != null;

    public void Plant(GameObject flower, Vector3 position)
    {
        if (flower == null) return;

        plantedFlower = flower;

        // Set position and parent
        flower.transform.position = position;
        flower.transform.SetParent(null); // Important! Don't keep it parented to the holdPoint

        // Reactivate collider & visuals
        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        flower.SetActive(true); // Make sure it's visible
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject flower = plantedFlower;
        plantedFlower = null;

        // Temporarily disable collider so it doesn’t instantly re-trigger
        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.transform.SetParent(null); // Important cleanup
        return flower;
    }

    public Transform GetPlantingPoint()
    {
        return plantingPoint != null ? plantingPoint : transform;
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = active ? Color.white : new Color(0.8f, 0.8f, 0.8f);
        }
    }
}
