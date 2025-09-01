using UnityEngine;

public class HealthCollectiblePulsate : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float pulseSpeed = 2f;
    public float rotationSpeed = 50f;

    private float scaleDifference;

    void Start()
    {
        scaleDifference = maxScale - minScale;
    }

    void Update()
    {
        // Pulsate scale
        float scale = minScale + Mathf.PingPong(Time.time * pulseSpeed, scaleDifference);
        transform.localScale = new Vector3(scale, scale, 1f);

        // Rotate
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
