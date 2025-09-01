using UnityEngine;

public class FollowAndFlip : MonoBehaviour
{
    public Transform target;            // Luna
    public float followSpeed    = 5f;
    public float followDistance = 0.7f;

    private bool  isFollowing    = false;
    private float originalScaleX;

    void Awake()
    {
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            ToggleFollow();

        if (isFollowing)
        {
            FollowLuna();
            FaceTarget();
        }
    }

    void FollowLuna()
    {
        if (target == null) return;
        Vector3 goal = target.position + Vector3.up * followDistance;
        transform.position = Vector3.Lerp(transform.position, goal, followSpeed * Time.deltaTime);
    }

    void FaceTarget()
    {
        if (target == null) return;
        bool shouldFaceRight = (target.position.x > transform.position.x);
        Vector3 s = transform.localScale;
        s.x = originalScaleX * (shouldFaceRight ? +1f : -1f);
        transform.localScale = s;
    }

    void ToggleFollow()
    {
        if (isFollowing) StopFollowing();
        else             StartFollowing();
    }

    void StartFollowing()
    {
        isFollowing = true;
    }

    void StopFollowing()
    {
        isFollowing = false;
        // start lowering to Luna’s exact Y-position
        StopAllCoroutines();
        StartCoroutine(LowerToLunaLevel());
    }

    System.Collections.IEnumerator LowerToLunaLevel()
    {
        if (target == null) yield break;
        // compute the final goal at exactly Luna’s Y
        Vector3 goal = new Vector3(
            transform.position.x,
            target.position.y,
            transform.position.z);

        while (Mathf.Abs(transform.position.y - goal.y) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, goal, followSpeed * Time.deltaTime);
            yield return null;
        }
        // ensure exact snap
        transform.position = goal;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform == target && !isFollowing)
            FaceTarget();
    }

    public void SetFollow(bool follow)
    {
        if (follow) StartFollowing();
        else        StopFollowing();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}


/* using UnityEngine;

public class FollowAndFlip : MonoBehaviour
{
    public Transform target; // The player (Luna)
    public float followSpeed = 5f; // Speed at which butterfly follows
    public float followDistance = 0.7f; // Distance above Luna when following
    public float lowerDistance = 0.3f; // Distance to lower when unfollowing
    private bool isFacingRight = false; // Default direction
    private bool isFollowing = false; // Tracks if the butterfly is following

    void Update()
    {
        // Toggle follow state with F key
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleFollow();
        }

        // If following, move the butterfly and update its sprite direction
        if (isFollowing)
        {
            FollowLuna();
            FlipSpriteIfNeeded();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // When not following, face Luna on collision
        if (!isFollowing && collision.transform == target)
        {
            FaceLunaOnCollision();
        }
    }

    void FollowLuna()
    {
        if (target == null)
        {
            Debug.LogWarning("Follow target not assigned.");
            return;
        }

        // Calculate the target position, offset by followDistance above
        Vector3 targetPosition = target.position + (Vector3.up * followDistance);

        // Smoothly move towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void FlipSpriteIfNeeded()
    {
        if (target == null)
        {
            return;
        }

        // Determine direction relative to the target
        float direction = target.position.x - transform.position.x;

        // Flip logic
        if (direction < 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction > 0 && isFacingRight)
        {
            Flip();
        }
    }

    void FaceLunaOnCollision()
    {
        if (target == null)
        {
            Debug.LogWarning("Target (Luna) not assigned.");
            return;
        }

        // Get the direction to Luna
        Vector3 directionToLuna = target.position - transform.position;

        // Rotate to face Luna based on position
        if (directionToLuna.x < 0 && !isFacingRight)
        {
            Flip();
        }
        else if (directionToLuna.x > 0 && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        // Toggle the facing direction
        isFacingRight = !isFacingRight;

        // Invert the local scale for flipping
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        
    }

    void ToggleFollow()
    {
        if (isFollowing)
        {
            StopFollowing();
        }
        else
        {
            StartFollowing();
        }
    }

    void StartFollowing()
    {
        isFollowing = true;
       
    }

    void StopFollowing()
    {
        isFollowing = false;
        
        StartCoroutine(LowerToOriginalOffset());
    }

    System.Collections.IEnumerator LowerToOriginalOffset()
    {
        // Lower butterfly by a fixed amount relative to its current height
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y - lowerDistance, transform.position.z);

        while (Mathf.Abs(transform.position.y - targetPosition.y) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition; // Ensure exact position
        Debug.Log("Butterfly lowered by fixed amount after unfollowing.");
    }

    public void SetFollow(bool follow)
    {
        if (!follow)
        {
            StopFollowing();
        }
        else
        {
            StartFollowing();
        }

        Debug.Log("Follow state set to: " + follow);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("New follow target assigned: " + (newTarget ? newTarget.name : "None"));
    }
}
*/