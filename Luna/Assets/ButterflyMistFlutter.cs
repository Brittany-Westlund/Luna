using UnityEngine;

public class ButterflyMistFlutter : MonoBehaviour
{
    [Header("References")]
    public Animator butterflyAnimator;
    public ButterflyFatigue butterflyFatigue;
    public ButterflyPerchAndFatigue perch;

    [Header("Animation")]
    public float normalSpeed = 1f;
    public float flutterSpeed = 2.2f;

    [Header("Fatigue")]
    public float fatigueTickInterval = 0.4f;
    public bool affectColor = true;

    private float _timer = 0f;
    private bool _isFluttering = false;

    void Awake()
    {
        if (!butterflyAnimator)
            butterflyAnimator = GetComponent<Animator>();

        if (!butterflyFatigue)
            butterflyFatigue = GetComponent<ButterflyFatigue>();

        if (!perch)
            perch = GetComponent<ButterflyPerchAndFatigue>();
    }

    void Update()
    {
        if (!_isFluttering)
            return;

        if (IsBlocked())
        {
            StopFlutter();
            return;
        }

        _timer += Time.deltaTime;

        if (_timer >= fatigueTickInterval)
        {
            _timer = 0f;
            butterflyFatigue.ApplyFatigue(affectColor);

            if (butterflyFatigue.IsExhausted())
            {
                StopFlutter();
                return;
            }
        }
    }

    public void StartFlutter()
    {
        if (IsBlocked())
            return;

        _isFluttering = true;
        _timer = 0f;

        if (butterflyAnimator)
            butterflyAnimator.speed = flutterSpeed;
    }

    public void StopFlutter()
    {
        _isFluttering = false;
        _timer = 0f;

        if (butterflyAnimator)
            butterflyAnimator.speed = normalSpeed;
    }

    public bool IsFluttering()
    {
        return _isFluttering;
    }

    bool IsBlocked()
    {
        if (butterflyFatigue != null && butterflyFatigue.IsExhausted())
            return true;

        if (perch != null && (perch.IsPerched() || perch.IsPerching()))
            return true;

        return false;
    }
}