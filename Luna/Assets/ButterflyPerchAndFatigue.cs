using UnityEngine;

public class ButterflyPerchAndFatigue : MonoBehaviour
{
    [Header("References")]
    public Transform butterfly;
    public Transform butterflyPerchPoint;
    public ButterflyFatigue butterflyFatigue;
    public FollowAndFlip followAndFlip;
    public Animator butterflyAnimator;
    public GameObject luna;
    public SpriteRenderer butterflyRenderer;
    public SpriteRenderer lunaRenderer;

    [Header("Perch Facing")]
    public bool useLunaRendererFacingWhilePerched = true;
    public bool invertPerchFacing = true;

    [Header("Perch Movement")]
    public float perchMoveSpeed = 4f;
    public float perchStopDistance = 0.05f;

    [Header("Animation")]
    public float perchedAnimatorSpeed = 0.6f;
    public float normalAnimatorSpeed = 1f;

    [Header("State")]
    public bool startPerched = false;

    private bool _isPerching = false;
    private bool _isPerched = false;
    private bool _isParentedToPerch = false;
    private Transform _originalParent;

    void Start()
    {
        if (butterfly == null)
            butterfly = transform;

        if (butterflyFatigue == null)
            butterflyFatigue = GetComponent<ButterflyFatigue>();

        if (followAndFlip == null)
            followAndFlip = GetComponent<FollowAndFlip>();

        if (butterflyAnimator == null)
            butterflyAnimator = GetComponent<Animator>();

        if (butterflyRenderer == null)
            butterflyRenderer = GetComponent<SpriteRenderer>();

        if (luna == null)
            luna = GameObject.FindWithTag("Player");

        if (lunaRenderer == null && luna != null)
            lunaRenderer = luna.GetComponentInChildren<SpriteRenderer>();

        _originalParent = butterfly.parent;

        if (startPerched && butterflyPerchPoint != null)
        {
            SnapAndParentToPerch();
            _isPerched = true;
            _isPerching = false;

            if (followAndFlip != null)
                followAndFlip.enabled = false;

            if (butterflyAnimator != null)
                butterflyAnimator.speed = perchedAnimatorSpeed;
        }
    }

    void Update()
    {
        if (butterfly == null || butterflyFatigue == null)
            return;

        if (butterflyFatigue.IsExhausted())
        {
            HandleExhaustedState();
        }
        else
        {
            HandleRecoveredState();
        }

        if (_isPerched)
        {
            UpdatePerchedFacing();
        }
    }

    void HandleExhaustedState()
    {
        if (followAndFlip != null && followAndFlip.enabled)
            followAndFlip.enabled = false;

        if (butterflyPerchPoint == null)
        {
            _isPerching = false;
            _isPerched = true;

            if (butterflyAnimator != null)
                butterflyAnimator.speed = perchedAnimatorSpeed;

            return;
        }

        if (_isParentedToPerch)
        {
            _isPerching = false;
            _isPerched = true;

            if (butterflyAnimator != null)
                butterflyAnimator.speed = perchedAnimatorSpeed;

            return;
        }

        Vector3 target = butterflyPerchPoint.position;
        target.z = butterfly.position.z;

        float dist = Vector2.Distance(butterfly.position, butterflyPerchPoint.position);

        if (dist > perchStopDistance)
        {
            _isPerching = true;
            _isPerched = false;

            butterfly.position = Vector3.MoveTowards(
                butterfly.position,
                target,
                perchMoveSpeed * Time.deltaTime
            );

            if (butterflyAnimator != null)
                butterflyAnimator.speed = normalAnimatorSpeed;
        }
        else
        {
            SnapAndParentToPerch();
            _isPerching = false;
            _isPerched = true;

            if (butterflyAnimator != null)
                butterflyAnimator.speed = perchedAnimatorSpeed;
        }
    }

    void HandleRecoveredState()
    {
        if (_isPerching || _isPerched || _isParentedToPerch)
        {
            UnparentFromPerch();

            _isPerching = false;
            _isPerched = false;

            if (followAndFlip != null && !followAndFlip.enabled)
                followAndFlip.enabled = true;

            if (butterflyAnimator != null)
                butterflyAnimator.speed = normalAnimatorSpeed;
        }
    }

    void SnapAndParentToPerch()
    {
        if (butterfly == null || butterflyPerchPoint == null)
            return;

        butterfly.SetParent(butterflyPerchPoint);
        butterfly.localPosition = Vector3.zero;
        butterfly.localRotation = Quaternion.identity;
        _isParentedToPerch = true;
    }

    void UnparentFromPerch()
    {
        if (butterfly == null)
            return;

        if (_isParentedToPerch)
        {
            butterfly.SetParent(_originalParent);
            _isParentedToPerch = false;
        }
    }

    void UpdatePerchedFacing()
    {
        if (butterflyRenderer == null)
            return;

        if (useLunaRendererFacingWhilePerched && lunaRenderer != null)
        {
            bool flip = lunaRenderer.flipX;

            if (invertPerchFacing)
                flip = !flip;

            butterflyRenderer.flipX = flip;
            return;
        }

        if (luna == null || butterfly == null)
            return;

        float lunaRelativeX = luna.transform.position.x - butterfly.position.x;

        if (lunaRelativeX > 0.02f)
            butterflyRenderer.flipX = invertPerchFacing ? false : true;
        else if (lunaRelativeX < -0.02f)
            butterflyRenderer.flipX = invertPerchFacing ? true : false;
    }

    public bool IsPerched()
    {
        return _isPerched;
    }

    public bool IsPerching()
    {
        return _isPerching;
    }

    public bool IsUnavailableBecauseExhausted()
    {
        return butterflyFatigue != null && butterflyFatigue.IsExhausted();
    }
}