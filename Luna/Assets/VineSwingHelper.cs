using UnityEngine;
using MoreMountains.CorgiEngine;   // ✅ Corgi input manager
using MoreMountains.Tools;         // ✅ Utility helpers (safe to include)

[RequireComponent(typeof(Rigidbody2D), typeof(HingeJoint2D))]
public class VineSwingController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The rigidbody of the vine pod itself.")]
    public Rigidbody2D vineRigidbody;
    [Tooltip("Optional trigger collider used to detect if Luna is on the vine.")]
    public Collider2D vineTrigger;

    [Header("Swing Settings")]
    [Tooltip("How much torque is added when Luna moves horizontally.")]
    public float swingForce = 8f;
    [Tooltip("How quickly swing slows down each frame (0.9–1.0). Higher = smoother.")]
    [Range(0.9f, 1f)] public float damping = 0.995f;
    [Tooltip("Maximum angular speed to prevent chaotic motion.")]
    public float maxAngularVelocity = 50f;
    [Tooltip("How much auto-centering torque is applied when empty.")]
    public float returnStrength = 0.5f;

    [Header("Player Detection")]
    [Tooltip("Tag of the player object (e.g., 'Player').")]
    public string playerTag = "Player";

    private bool playerOnVine;
    private float inputValue;

    void Awake()
    {
        if (!vineRigidbody) vineRigidbody = GetComponent<Rigidbody2D>();

        // Auto-create trigger if missing
        if (vineTrigger == null)
        {
            GameObject trigger = new GameObject("VineTrigger");
            trigger.transform.SetParent(transform);
            trigger.transform.localPosition = Vector3.zero;

            var box = trigger.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1f, 0.5f);
            vineTrigger = box;
        }
    }

    void Update()
    {
        if (playerOnVine)
        {
            // ✅ Corgi input manager call
            inputValue = InputManager.Instance.PrimaryMovement.x;
            //Debug.Log("InputValue from Corgi: " + inputValue);
        }
        else inputValue = 0f;
    }

    void FixedUpdate()
    {
        // Apply player-driven torque
        if (Mathf.Abs(inputValue) > 0.05f)
        {
            vineRigidbody.AddTorque(-inputValue * swingForce, ForceMode2D.Force);
        }

        // Auto-center when no player
        if (!playerOnVine)
        {
            float angle = vineRigidbody.rotation % 360f;
            if (angle > 180f) angle -= 360f;
            float correction = -angle * returnStrength * Time.fixedDeltaTime;
            vineRigidbody.AddTorque(correction, ForceMode2D.Force);
        }

        // Damping and clamping
        vineRigidbody.angularVelocity *= damping;
        vineRigidbody.angularVelocity = Mathf.Clamp(vineRigidbody.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerOnVine = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerOnVine = false;
    }
}
