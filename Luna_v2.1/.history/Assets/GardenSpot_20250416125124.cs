using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [SerializeField] private Transform plantingPoint;
    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;

    private void Start()
    {
        // Auto-detect and register a flower already placed here
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Sprout"))
            {
                plantedFlower = col.gameObject;
                plantedFlower.transform.SetParent(null); // Just in case
                break;
            }
        }
    }


    public bool HasPlantedFlower => plantedFlower != null;

    public void Plant(GameObject flower, Vector3 position)
    {
        flower.transform.position = position;
        flower.transform.SetParent(null);

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isPlanted = true;
            sprout.isHeld = false;
        }

        plantedFlower = flower;
        flower.GetComponent<Collider2D>().enabled = true;
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
