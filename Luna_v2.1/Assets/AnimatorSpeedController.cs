using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedController : MonoBehaviour
{
    public Animator animator;
    public float speed = 2f; // Example: 2x speed

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.speed = speed;

        // Combine other logic from any duplicate Start methods here
    }
}