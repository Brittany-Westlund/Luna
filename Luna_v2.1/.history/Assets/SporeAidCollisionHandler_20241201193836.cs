using UnityEngine;
using UnityEngine.Events;

public class SporeAidCollisionHandler : MonoBehaviour
{
    [System.Serializable]
    public class CollisionEvent
    {
        public string eventName; // Name for the event
        public UnityEvent onCollisionEvent; // UnityEvent to trigger
    }

    public string friendlyTag = "Friendlies"; // Tag for friendly objects
    public CollisionEvent[] collisionEvents; // Array of named collision events

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(friendlyTag))
        {
            Debug.Log($"Spore collided with a friendly object: {collision.name}");

            // Trigger all events in the collisionEvents array
            foreach (var collisionEvent in collisionEvents)
            {
                Debug.Log($"Triggering event: {collisionEvent.eventName}");
                collisionEvent.onCollisionEvent?.Invoke();
            }

            // Destroy the spore after events are triggered
            Destroy(gameObject);
        }
    }
}
