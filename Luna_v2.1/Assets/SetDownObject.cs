using UnityEngine;

public class SetDownObject : MonoBehaviour
{
    private PickupObject carriedObject;

    private void Start()
    {
        // Initialize carriedObject to null
        carriedObject = null;
    }

    // This method is called when the player collides with a 2D collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player is carrying an object
        if (carriedObject != null)
        {
            // Check if the other collider is the designated set down location
            if (other.CompareTag("SetDownLocation")) // Make sure the collider has the tag "SetDownLocation"
            {
                carriedObject.SetDown();
                carriedObject = null; // Clear the reference
            }
        }
    }

    // Call this method from the PickupObject script when the player picks up an object
    public void PickUpObject(PickupObject objectToCarry)
    {
        carriedObject = objectToCarry;
    }
}
