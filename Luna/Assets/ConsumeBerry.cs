using UnityEngine;

public class ConsumeBerry : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Only consume this flower type (e.g. TeaRose). Leave empty to accept any.")]
    public string requiredFlowerType = "TeaRose";

    [Tooltip("Name of the player's hold point (where held flowers are parented).")]
    public string holdPointName = "HoldPoint";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Transform holdPoint = FindChildRecursive(other.transform, holdPointName);

        if (holdPoint == null)
        {
            Debug.LogWarning("[ConsumeBerry] HoldPoint not found on player.");
            return;
        }

        // Look for a held flower under the hold point
        FlowerPickup heldFlower = holdPoint.GetComponentInChildren<FlowerPickup>();

        if (heldFlower == null)
            return;

        // Make sure it's actually being held
        if (!heldFlower.IsHeld)
            return;

        // Optional type check
        if (!string.IsNullOrEmpty(requiredFlowerType) &&
            heldFlower.flowerType != requiredFlowerType)
        {
            return;
        }

        Debug.Log($"🍓 Consumed berry: {heldFlower.flowerType}");

        Destroy(heldFlower.gameObject);
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == targetName)
                return child;

            Transform found = FindChildRecursive(child, targetName);
            if (found != null)
                return found;
        }

        return null;
    }
}