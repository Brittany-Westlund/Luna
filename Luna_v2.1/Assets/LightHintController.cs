using UnityEngine;

public class LightHintController : MonoBehaviour
{
    public GameObject lightHintIcon; // Assign your light icon here
    private bool isPlayerNearby = false;
    private bool isFullyGrown = false;
    private bool hasBeenLit = false;

    public void SetFullyGrown(bool value)
    {
        isFullyGrown = value;
    }

    public void Deactivate()
    {
        hasBeenLit = true;
        if (lightHintIcon != null)
        {
            lightHintIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (lightHintIcon != null)
        {
            lightHintIcon.SetActive(isPlayerNearby && isFullyGrown && !hasBeenLit);
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
