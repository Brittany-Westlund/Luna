using UnityEngine;

public class AutoCenterOnPlatform : MonoBehaviour
{
    public GameObject movingPlatform; // The moving platform the character will center on
    public bool faceRightOnPlatform; // Should the character face right when on the platform

    private bool isOnPlatform = false;
    
    // Update is called once per frame
    void Update()
    {
        if (isOnPlatform && movingPlatform != null)
        {
            // Center the character on the platform
            Vector3 platformCenter = movingPlatform.transform.position;
            // Adjust y position to be on top of the platform, add the platform's collider bounds extents if needed
            platformCenter.y += (movingPlatform.GetComponent<Collider2D>().bounds.extents.y + GetComponent<Collider2D>().bounds.extents.y);
            transform.position = platformCenter;
            
            // Set the character's direction
            if (faceRightOnPlatform)
            {
                // Assuming that facing right means scale.x > 0
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                // Assuming that facing left means scale.x < 0
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == movingPlatform)
        {
            // The character is now on the platform
            isOnPlatform = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == movingPlatform)
        {
            // The character has left the platform
            isOnPlatform = false;
        }
    }
}
