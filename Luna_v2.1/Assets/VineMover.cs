using UnityEngine;

public class VineMover : MonoBehaviour
{
    public GameObject vine; // The vine GameObject
    public float swingAmplitude = 30f; // The maximum angle the vine swings to (in degrees)
    public float swingSpeed = 1f; // The speed of the swing

    private float startRotationZ; // Initial rotation angle

    private void Start()
    {
        // Store the initial Z rotation of the vine
        startRotationZ = vine.transform.rotation.eulerAngles.z;
    }

    private void Update()
    {
        // Calculate the swing angle using a sine wave
        float swingAngle = swingAmplitude * Mathf.Sin(Time.time * swingSpeed);

        // Apply the swing angle to the vine's rotation, keeping the original X and Y rotation, modifying only Z
        vine.transform.rotation = Quaternion.Euler(new Vector3(0, 0, startRotationZ + swingAngle));
    }
}
