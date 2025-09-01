using UnityEngine;
using System.Collections;

public class VineMover : MonoBehaviour
{
    public GameObject vine; // Attach your vine GameObject here in the Inspector
    public Vector3 targetPosition; // Set the target position for the vine in the Inspector
    public float speed = 1f; // Set the speed of the vine's movement

    private void Update()
    {
   
            // Start moving the vine
            StartCoroutine(MoveVine());
    
        
    }

    private IEnumerator MoveVine()
    {
        while (Vector3.Distance(vine.transform.position, targetPosition) > 0.01f)
        {
            vine.transform.position = Vector3.MoveTowards(vine.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
}