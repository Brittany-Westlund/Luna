using UnityEngine;
using System.Collections;

public class TrampolineSporeMushroom : MonoBehaviour
{
    [Header("Spore Settings")]
    public GameObject sporePrefab;
    public Transform spawnPoint;
    public float cooldownDuration = 2f;

    private bool isOnCooldown = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOnCooldown)
        {
            SpawnSpore();
            StartCoroutine(SporeCooldown());
        }
    }

    private void SpawnSpore()
    {
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Instantiate(sporePrefab, position, Quaternion.identity);
        Debug.Log("🌱 Spore released from trampoline mushroom!");
    }

    private IEnumerator SporeCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }
}
