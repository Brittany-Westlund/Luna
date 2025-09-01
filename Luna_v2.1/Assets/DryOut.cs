using UnityEngine;

public class DryingOut : MonoBehaviour
{
    public float initialDryOutDuration = 10f; // Initial time in seconds before the object starts drying out
    public float reductionSpeed = 0.1f; // Speed at which the object shrinks
    public float shrinkAmount = 0.1f; // Amount by which the object shrinks
    public float yPositionDecrease = 0.05f; // Amount by which the Y position decreases

    private float dryOutTimer; // Timer to track drying out duration

    void Start()
    {
        ResetDryOutTimer();
    }

    void Update()
    {
        // Count down the timer
        dryOutTimer -= Time.deltaTime;

        // Check if it's time to start drying out
        if (dryOutTimer <= 0f)
        {
            // Reduce scale
            Vector3 newScale = transform.localScale - new Vector3(shrinkAmount, shrinkAmount, 0f) * reductionSpeed * Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Max(newScale.x, 0), Mathf.Max(newScale.y, 0), newScale.z);

            // Decrease Y position
            transform.position += new Vector3(0, -yPositionDecrease * reductionSpeed * Time.deltaTime, 0);

            // Deactivate if shrunk completely
            if (transform.localScale.x <= 0 || transform.localScale.y <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    // Method to be called when the object grows in size due to watering
    public void ResetOnGrowth()
    {
        ResetDryOutTimer();
    }

    private void ResetDryOutTimer()
    {
        dryOutTimer = initialDryOutDuration;
    }
}
