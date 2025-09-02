// FoxglovePollenPickup.cs
using UnityEngine;
using System.Collections;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float pollenDuration = 30f; // How long the effect lasts

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Transform iconTransform = other.transform.Find("FoxglovePollenHoldPoint/FoxglovePollenLuna");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("FoxglovePollenLuna not found under Player.");
        }

        var effect = other.GetComponent<FoxglovePollenEffect>();
        if (effect != null)
        {
            effect.ActivatePollenEffect(pollenDuration);
        }

        Destroy(gameObject);
    }
}