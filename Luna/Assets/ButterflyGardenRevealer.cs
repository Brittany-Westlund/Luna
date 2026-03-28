using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButterflyGardenRevealer : MonoBehaviour
{
    [Header("Dependencies")]
    public ButterflyFatigue fatigue;
    public Animator butterflyAnimator;

    [Header("Reveal Strength")]
    [Tooltip("Minimum animator speed required to reveal gardens. With your current fatigue setup, 0.7 means steps 0-3 can reveal, while 4-5 cannot.")]
    public float minRevealSpeed = 0.7f;

    [Tooltip("How often to scan for nearby gardens.")]
    public float checkInterval = 0.2f;

    [Tooltip("Range of wing-wind influence.")]
    public float revealRadius = 1.2f;

    [Header("Reveal Rules")]
    [Tooltip("If true, gardens that require butterfly reveal will stay revealed once discovered.")]
    public bool stayRevealedOnceDiscovered = true;

    [Tooltip("If false, non-persistent reveal gardens hide again when butterfly strength drops too low or leaves range.")]
    public bool hideUnrevealedGardensWhenTooWeak = true;

    [Tooltip("If true, exhausted butterflies can never reveal gardens.")]
    public bool exhaustedBlocksReveal = true;

    [Header("Debug")]
    public bool debugLogs = false;

    private readonly HashSet<GardenSpot> _revealedGardens = new HashSet<GardenSpot>();

    private void Start()
    {
        if (fatigue == null)
            fatigue = GetComponent<ButterflyFatigue>();

        if (butterflyAnimator == null)
            butterflyAnimator = GetComponent<Animator>();

        StartCoroutine(CheckRevealLoop());
    }

    private IEnumerator CheckRevealLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            bool canReveal = CanRevealGardens();

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, revealRadius);
            HashSet<GardenSpot> gardensInRange = new HashSet<GardenSpot>();

            foreach (Collider2D hit in hits)
            {
                if (hit == null || !hit.CompareTag("Garden"))
                    continue;

                GardenSpot garden = hit.GetComponent<GardenSpot>();
                if (garden == null)
                    continue;

                gardensInRange.Add(garden);

                if (!garden.requiresButterflyReveal)
                {
                    // This garden is always visible by design.
                    continue;
                }

                if (canReveal)
                {
                    garden.Reveal();

                    if (stayRevealedOnceDiscovered)
                        _revealedGardens.Add(garden);

                    if (debugLogs)
                        Debug.Log($"[ButterflyGardenRevealer] Revealed garden: {garden.name}");
                }
                else
                {
                    bool shouldStayVisible = stayRevealedOnceDiscovered && _revealedGardens.Contains(garden);

                    if (!shouldStayVisible && hideUnrevealedGardensWhenTooWeak)
                    {
                        garden.Hide();

                        if (debugLogs)
                            Debug.Log($"[ButterflyGardenRevealer] Hid unrevealed garden due to weak flutter: {garden.name}");
                    }
                }
            }

            if (hideUnrevealedGardensWhenTooWeak)
            {
                CleanupOutOfRangeRevealState(gardensInRange);
            }

            yield return wait;
        }
    }

    private bool CanRevealGardens()
    {
        if (butterflyAnimator == null)
            return false;

        if (exhaustedBlocksReveal && fatigue != null && fatigue.IsExhausted())
            return false;

        return butterflyAnimator.speed >= minRevealSpeed;
    }

    private void CleanupOutOfRangeRevealState(HashSet<GardenSpot> gardensInRange)
    {
        GardenSpot[] allGardens = FindObjectsOfType<GardenSpot>(includeInactive: true);

        for (int i = 0; i < allGardens.Length; i++)
        {
            GardenSpot garden = allGardens[i];
            if (garden == null)
                continue;

            if (!garden.requiresButterflyReveal)
                continue;

            if (gardensInRange.Contains(garden))
                continue;

            bool shouldStayVisible = stayRevealedOnceDiscovered && _revealedGardens.Contains(garden);

            if (!shouldStayVisible)
            {
                garden.Hide();

                if (debugLogs)
                    Debug.Log($"[ButterflyGardenRevealer] Hid out-of-range garden: {garden.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, revealRadius);
    }
}