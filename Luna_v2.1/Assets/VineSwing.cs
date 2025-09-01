using UnityEngine;
using System.Collections;

public class VineSwing : MonoBehaviour
{
    public float angle = 30.0f;
    public float speed = 2.0f;

    private Quaternion _start, _end;

    void Start()
    {
        _start = Quaternion.AngleAxis(angle, Vector3.forward);
        _end = Quaternion.AngleAxis(-angle, Vector3.forward);

        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        while (true)
        {
            yield return StartCoroutine(SwingMove(_start, _end));
            yield return StartCoroutine(SwingMove(_end, _start));
        }
    }

    IEnumerator SwingMove(Quaternion from, Quaternion to)
    {
        float elapsed = 0.0f;
        float duration = Mathf.PI / speed; // Half period of sine wave

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float phase = Mathf.Sin(elapsed * speed);
            transform.localRotation = Quaternion.Lerp(from, to, (phase + 1.0f) / 2.0f);
            yield return null;
        }
    }
}
