using UnityEngine;
using System.Collections;

public class ConsumeBerry : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Only consume this flower type (e.g. TeaRose). Leave empty to accept any.")]
    public string requiredFlowerType = "TeaRose";

    [Tooltip("Name of the player's hold point (where held flowers are parented).")]
    public string holdPointName = "HoldPoint";

    [Header("Deer Visual Feedback")]
    [Tooltip("Child object on the deer to show when it steals the flower.")]
    public GameObject stolenFlowerVisual;

    [Tooltip("How long the stolen flower visual stays active on the deer.")]
    public float stolenFlowerVisualDuration = 2f;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip stealSFX;

    private Coroutine stolenFlowerRoutine;

    private void Awake()
    {
        if (stolenFlowerVisual != null)
            stolenFlowerVisual.SetActive(false);
    }

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

        ShowStolenFlowerVisual();
        PlayStealSFX();

        Destroy(heldFlower.gameObject);
    }

    private void ShowStolenFlowerVisual()
    {
        if (stolenFlowerVisual == null)
            return;

        if (stolenFlowerRoutine != null)
            StopCoroutine(stolenFlowerRoutine);

        stolenFlowerRoutine = StartCoroutine(StolenFlowerVisualRoutine());
    }

    private IEnumerator StolenFlowerVisualRoutine()
    {
        stolenFlowerVisual.SetActive(true);

        yield return new WaitForSeconds(stolenFlowerVisualDuration);

        if (stolenFlowerVisual != null)
            stolenFlowerVisual.SetActive(false);

        stolenFlowerRoutine = null;
    }

    private void PlayStealSFX()
    {
        if (audioSource != null && stealSFX != null)
        {
            audioSource.PlayOneShot(stealSFX);
        }
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