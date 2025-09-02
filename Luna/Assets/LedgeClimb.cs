using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeClimb : MonoBehaviour
{
    // We need to tell Unity which animations to use, so we keep track of the Animator.
    public Animator animator;

    // We decide how long to wait before climbing. Let's say 2 seconds for now.
    public float climbDelay = 2f;

    // Unity calls OnTriggerEnter2D when our character touches something tagged as a "Ledge".
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // We check if the tag of what we touched is "Ledge".
        if (collision.CompareTag("Ledge"))
        {
            // If it is, we start our climbing process after a short wait.
            StartCoroutine(ClimbAfterDelay());
        }
    }

    // This is a special Unity function that can wait for a while.
    private IEnumerator ClimbAfterDelay()
    {
        // We tell Unity to wait for the number of seconds we set earlier.
        yield return new WaitForSeconds(climbDelay);
        
        // Then we tell our Animator to start the climb animation.
        animator.SetTrigger("Climb");

        // Unity's Animator will take care of the rest, going back to idle when climb is done.
    }
}
