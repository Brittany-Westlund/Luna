using UnityEngine;
using System.Collections;

public class TrampolineSporeMushroom : MonoBehaviour
{
    [Header("Spore Settings")]
    public GameObject sporePrefab;
    public Transform spawnPoint; // Where the spore appears (optional)
    public float cooldownDuration = 2f;

    private bool isOnCooldown = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && !isOnCooldown)
        {
            SpawnSpore();
            StartCoroutine(SporeCooldown());
        }
    }

    private void SpawnSpore()
    {
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Instantiate(sporePrefab, position, Quaternion.identity);
        Debug.Log("Spore released from mushroom!");
    }

    private IEnumerator SporeCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }
}
