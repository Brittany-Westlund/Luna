using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class FlowerSway : MonoBehaviour
{
    public float swayAmount = 5f;
    public float swaySpeed = 2f;
    public Transform pivotPoint;

    private GameObject player;
    private CorgiController playerController;
    private float currentSwayAngle = 0f;
    private Coroutine swayCoroutine;
    private const float angleTolerance = 0.01f;
    
    // 👇 NEW: this flag prevents swaying after pickup
    public bool isBeingPickedUp = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<CorgiController>();
        }
    }

    private IEnumerator UpdateSway(bool isPlayerMoving)
    {
        while (!isBeingPickedUp) // 👈 Exit immediately if picked up
        {
            float targetSwayAngle = (isPlayerMoving && playerController != null && playerController.Speed.magnitude > 0.1f)
                ? Mathf.Sin(Time.time * swaySpeed) * swayAmount
                : 0f;

            currentSwayAngle = Mathf.Lerp(currentSwayAngle, targetSwayAngle, Time.deltaTime * swaySpeed);
            float angleDifference = currentSwayAngle - transform.localEulerAngles.z;
            transform.RotateAround(pivotPoint.position, Vector3.forward, angleDifference);

            if (Mathf.Abs(currentSwayAngle - targetSwayAngle) < angleTolerance)
                break;

            yield return null;
        }

        swayCoroutine = null;
    }

    private void StartSwayCoroutine(bool isPlayerMoving)
    {
        if (isBeingPickedUp) return; // 👈 Skip entirely if picked up

        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }

        swayCoroutine = StartCoroutine(UpdateSway(isPlayerMoving));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartSwayCoroutine(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartSwayCoroutine(false);
        }
    }
}
