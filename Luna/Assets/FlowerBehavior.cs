using UnityEngine;

public class FlowerBehavior : MonoBehaviour
{
    private bool isFullyGrown = false; // Track whether the flower is fully grown
    private SpriteRenderer litFlowerRenderer;

    private void Start()
    {
        // Assign litFlowerRenderer if it exists on the child object
        litFlowerRenderer = GetComponentInChildren<SpriteRenderer>();
        if (litFlowerRenderer != null)
        {
            litFlowerRenderer.enabled = false; // Start with the lit flower disabled
        }
    }

    public bool IsFullyGrown()
    {
        return isFullyGrown;
    }

    public void SetFullyGrown(bool grown)
    {
        isFullyGrown = grown;
    }

    public void ActivateLight()
    {
        if (litFlowerRenderer != null)
        {
            litFlowerRenderer.enabled = true; // Enable the lit flower's SpriteRenderer
            Debug.Log($"Flower {name} has been lit!");
        }
        else
        {
            Debug.LogWarning($"Flower {name} does not have a litFlowerRenderer assigned.");
        }
    }
}
