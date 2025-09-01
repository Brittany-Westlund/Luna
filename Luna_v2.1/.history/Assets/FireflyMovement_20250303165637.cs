using UnityEngine;
using System.Collections;

public class FireflyMovement : MonoBehaviour
{
    public float moveDistance = 3f; // How far the Firefly should move
    public float speed = 3f; // Movement speed

    public void MoveAway()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        Vector3 targetPosition = transform.position + new Vector3(moveDistance, 0, 0);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
}
