using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float speed = 2f;        // How fast it pulses
    public float amount = 0.1f;     // How big the pulse is (0.1 = 10%)

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * speed) * amount;
        transform.localScale = baseScale * pulse;
    }
}