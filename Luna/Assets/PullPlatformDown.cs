using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    public class PullPlatformDown : MonoBehaviour
    {
        public float pullDownSpeed = 0.5f; // Speed at which the character pulls the platform down
        public float minPlatformHeight = 1f; // Minimum height of the platform
        public string playerTag = "Player"; // Tag used to identify the player

        private Rigidbody2D platformRigidbody;
        private bool isPullingDown = false;
        private GameObject player; // Reference to the player game object

        void Start()
        {
            platformRigidbody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (isPullingDown)
            {
                PullDownPlatform();
            }
        }

        private void PullDownPlatform()
        {
            if (transform.position.y > minPlatformHeight)
            {
                platformRigidbody.velocity = new Vector2(platformRigidbody.velocity.x, -pullDownSpeed);
            }
            else
            {
                StopPullingDown();
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag(playerTag))
            {
                player = collider.gameObject;
                isPullingDown = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject == player)
            {
                StopPullingDown();
                player = null; // Clear the player reference
            }
        }

        private void StopPullingDown()
        {
            isPullingDown = false;
            platformRigidbody.velocity = new Vector2(platformRigidbody.velocity.x, 0); // Stop downward movement
            // If you have a Character script attached to the player, you may want to reset the vertical speed there as well
            // For example:
            // if (player != null)
            // {
            //     var character = player.GetComponent<Character>();
            //     if (character != null)
            //     {
            //         character.ResetVerticalSpeed();
            //     }
            // }
        }
    }
}
