using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private string collectibleID;

    private void Start()
    {
       Debug.Log($"[Collectible] {name} awake in scene. ID = {collectibleID}");
    bool collected = CollectibleManager.Instance.HasCollected(collectibleID);
    Debug.Log($"[Collectible] {collectibleID} collected? {collected}");
    if (collected)
        gameObject.SetActive(false);
       
        // Hide this object if it's already been collected
        if (CollectibleManager.Instance.HasCollected(collectibleID))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectibleManager.Instance.MarkCollected(collectibleID);
            gameObject.SetActive(false);
        }
    }
}
