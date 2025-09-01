using UnityEngine;

public class LunaRest : MonoBehaviour
{
    public float restRate = 0.2f; // Health per second (tweak this!)
    public float maxHealth = 1f;
    public float currentHealth = 0.5f; // Example starting point

    public GameObject lunaRestingSprite; // Assign in Inspector
    public SpriteRenderer normalSprite;

    private bool isInGarden = false;
    private bool isResting = false;
    private Vector2 lastPosition;

    void Update()
    {
        if (isInGarden && Input.GetKeyDown(KeyCode.Z) && !isResting)
        {
            StartResting();
        }

        if (isResting)
        {
            // Refill health
            currentHealth = Mathf.Clamp(currentHealth + restRate * Time.deltaTime, 0f, maxHealth);

            // Detect movement to cancel
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            if ((currentPosition - lastPosition).magnitude > 0.01f)
            {
                StopResting();
            }

            lastPosition = currentPosition;
        }
    }

    void StartResting()
    {
        isResting = true;
        lastPosition = new Vector2(transform.position.x, transform.position.y);

        if (lunaRestingSprite != null) lunaRestingSprite.SetActive(true);
        if (normalSprite != null) normalSprite.enabled = false;

        Debug.Log("🌿 Luna is resting...");
    }

    void StopResting()
    {
        isResting = false;

        if (lunaRestingSprite != null) lunaRestingSprite.SetActive(false);
        if (normalSprite != null) normalSprite.enabled = true;

        Debug.Log("🌿 Luna stopped resting.");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = false;
            if (isResting)
            {
                StopResting();
            }
        }
    }
}
