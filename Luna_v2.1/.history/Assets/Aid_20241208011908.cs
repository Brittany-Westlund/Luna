using UnityEngine;
using System.Collections;

public class Assist : MonoBehaviour
{
    public float aidDistance = 3f;
    public float moveSpeed = 5f;

    private PresentSpore presentSpore;

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && presentSpore.HasSporeAttached && !presentSpore.IsSliding)
        {
            GameObject attachedSpore = presentSpore.GetAttachedSpore();
            if (attachedSpore != null)
            {
                StartCoroutine(SlideSporeForward(attachedSpore));
            }
        }
    }

    private IEnumerator SlideSporeForward(GameObject spore)
    {
        Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 targetPosition = spore.transform.position + direction * aidDistance;

        spore.transform.SetParent(null);

        while (Vector3.Distance(spore.transform.position, targetPosition) > 0.01f)
        {
            spore.transform.position = Vector3.MoveTowards(
                spore.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        Collider2D hitCollider = Physics2D.OverlapCircle(targetPosition, 0.5f);
        if (hitCollider != null && hitCollider.CompareTag("Butterfly"))
        {
            ButterflyFlyHandler butterflyFlyHandler = hitCollider.GetComponent<ButterflyFlyHandler>();
            if (butterflyFlyHandler != null)
            {
                butterflyFlyHandler.ResetCooldownExplicitly(); // Explicitly reset cooldown
            }
        }

        Destroy(spore);
        presentSpore.ResetSporeState();
    }
}
