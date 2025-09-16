using UnityEngine;

public class TeacupProximity : MonoBehaviour
{
    private bool playerNearby = false;

    public bool IsPlayerNearby => playerNearby;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
