using UnityEngine;

public class LightHintController : MonoBehaviour
{
    public GameObject lightHintIcon; // Assign your light icon here
    private bool isPlayerNearby = false;
    private bool isFullyGrown = false;

    public void SetFullyGrown(bool value)
    {
        isFullyGrown = value;
    }

    private void Update()
    {
        if (lightHintIcon != null)
        {
            lightHintIcon.SetActive(isPlayerNearby && isFullyGrown);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
