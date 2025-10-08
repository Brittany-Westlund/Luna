using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ButterflyGardenRevealer : MonoBehaviour
{
    [Header("Dependencies")]
    public ButterflyFatigue fatigue;           // drag your fatigue script here
    public Animator butterflyAnimator;         // reference to same animator used for wing speed
    public float minRevealSpeed = 0.8f;        // threshold of animator speed to reveal gardens
    public float checkInterval = 0.2f;         // how often to recheck nearby gardens
    public float revealRadius = 1.2f;          // range of wing wind influence

    private readonly List<GardenSpot> _nearbyGardens = new();

    void Start()
    {
        if (fatigue == null)
            fatigue = GetComponent<ButterflyFatigue>();
        if (butterflyAnimator == null)
            butterflyAnimator = GetComponent<Animator>();

        StartCoroutine(CheckRevealLoop());
    }

    IEnumerator CheckRevealLoop()
    {
        while (true)
        {
            float speed = butterflyAnimator.speed;
            bool canReveal = speed >= minRevealSpeed && !fatigue.IsExhausted();

            // scan for gardens in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, revealRadius);
            foreach (var hit in hits)
            {
               if (!hit.CompareTag("Garden")) continue;  // tag your GardenGrowing roots as "Garden"

                // find the "Sparkles" child
                Transform sparkles = hit.transform.Find("Sparkles");
                if (sparkles == null) continue;

                // toggle based on wing speed
                sparkles.gameObject.SetActive(canReveal);
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // Optional gizmo to visualize the range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, revealRadius);
    }
}
