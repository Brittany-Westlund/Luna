using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Ledge Hang Custom")]
    public class CharacterLedgeHangCustom : CharacterAbility, MMEventListener<LedgeEvent>
    {
        public override string HelpBoxText() { return "Custom ledge hang with adjustable climb start and final climb offsets."; }

        [Header("Animation")]
        public string IdleAnimationName = "Idle";
        public float ClimbingAnimationDuration = 0.5f;

        [Header("Settings")]
        public float MinimumHangingTime = 0.2f;

        [Header("Custom Climb Start")]
        [Tooltip("Applied relative to Luna's ACTUAL current hang position when climb starts. Negative Y moves her lower.")]
        public Vector3 ClimbStartOffset = Vector3.zero;

        [Header("Custom Final Climb")]
        [Tooltip("Applied on top of the ledge's ClimbOffset. Use this if the normal climb landing is too far right/up.")]
        public Vector3 FinalClimbOffset = Vector3.zero;

        protected Ledge _ledge = null;
        protected CharacterJump _characterJump;
        protected WaitForSeconds _climbingAnimationDelay;
        protected float _ledgeHangingStartedTimestamp;

        protected override void Initialization()
        {
            base.Initialization();
            _characterJump = _character?.FindAbility<CharacterJump>();
            _climbingAnimationDelay = new WaitForSeconds(ClimbingAnimationDuration);
        }

        protected override void HandleInput()
        {
            if (_movement.CurrentState != CharacterStates.MovementStates.LedgeHanging)
            {
                return;
            }

            if (Time.time - _ledgeHangingStartedTimestamp < MinimumHangingTime)
            {
                return;
            }

            if (_verticalInput > _inputManager.Threshold.y)
            {
                StartCoroutine(Climb());
            }
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            HandleLedge();

            if ((_movement.CurrentState != CharacterStates.MovementStates.LedgeHanging)
                && (_movement.CurrentState != CharacterStates.MovementStates.LedgeClimbing)
                && (_movement.PreviousState == CharacterStates.MovementStates.LedgeHanging))
            {
                DetachFromLedge();
            }
        }

        public virtual void OnMMEvent(LedgeEvent ledgeEvent)
        {
            if (ledgeEvent.CharacterCollider.gameObject != _character.gameObject)
            {
                return;
            }
            StartGrabbingLedge(ledgeEvent.LedgeGrabbed);
        }

        public virtual void StartGrabbingLedge(Ledge ledge)
        {
            if ((_character.IsFacingRight && (ledge.LedgeGrabDirection == Ledge.LedgeGrabDirections.Left))
                || (!_character.IsFacingRight && (ledge.LedgeGrabDirection == Ledge.LedgeGrabDirections.Right)))
            {
                return;
            }

            if (!AbilityAuthorized || (_movement.CurrentState == CharacterStates.MovementStates.Jetpacking))
            {
                return;
            }

            _ledgeHangingStartedTimestamp = Time.time;
            _ledge = ledge;

            _controller.CollisionsOff();
            PlayAbilityStartFeedbacks();

            _movement.ChangeState(CharacterStates.MovementStates.LedgeHanging);
            MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.LedgeHang, MMCharacterEvent.Moments.Start);
        }

        protected virtual void HandleLedge()
        {
            if (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging)
            {
                _controller.SetForce(Vector2.zero);
                _controller.GravityActive(false);

                if (_characterJump != null)
                {
                    _characterJump.ResetNumberOfJumps();
                }

                _characterHorizontalMovement.AbilityPermitted = false;
                _character.CanFlip = false;

                _controller.transform.position = _ledge.transform.position + _ledge.HangOffset;
            }
        }

        protected virtual IEnumerator Climb()
        {
            if (_ledge == null)
            {
                yield break;
            }

            // Cache everything NOW so nulls later can't break the climb
            Ledge cachedLedge = _ledge;
            Vector3 actualHangPosition = _character.transform.position;
            Vector3 climbStartPosition = actualHangPosition + ClimbStartOffset;
            Vector3 finalClimbPosition = cachedLedge.transform.position + cachedLedge.ClimbOffset + FinalClimbOffset;

            _movement.ChangeState(CharacterStates.MovementStates.LedgeClimbing);

            if (_animator != null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ledgeClimbingAnimationParameter, true, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
            }

            if (_inputManager != null)
            {
                _inputManager.InputDetectionActive = false;
            }

            // Start climb from where Luna ACTUALLY was hanging, not from HangOffset math
            _character.transform.position = climbStartPosition;

            yield return _climbingAnimationDelay;

            if (_inputManager != null)
            {
                _inputManager.InputDetectionActive = true;
            }

            if (_animator != null)
            {
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ledgeClimbingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
                MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, true, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
                _animator.Play(IdleAnimationName);
            }

            // Use cached final position so _ledge going null won't crash
            _character.transform.position = finalClimbPosition;

            _movement.ChangeState(CharacterStates.MovementStates.Idle);
            _controller.GravityActive(true);

            DetachFromLedge();
        }

        protected virtual void DetachFromLedge()
        {
            _ledge = null;
            _character.CanFlip = true;
            _characterHorizontalMovement.AbilityPermitted = true;
            _controller.CollisionsOn();

            if (_startFeedbackIsPlaying)
            {
                StopStartFeedbacks();
                PlayAbilityStopFeedbacks();
                MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.LedgeHang, MMCharacterEvent.Moments.End);
            }
        }

        protected const string _ledgeHangingAnimationParameterName = "LedgeHanging";
        protected const string _ledgeClimbingAnimationParameterName = "LedgeClimbing";

        protected int _ledgeHangingAnimationParameter;
        protected int _ledgeClimbingAnimationParameter;
        protected int _idleAnimationParameter;

        protected override void InitializeAnimatorParameters()
        {
            _idleAnimationParameter = Animator.StringToHash(IdleAnimationName);
            RegisterAnimatorParameter(_ledgeHangingAnimationParameterName, AnimatorControllerParameterType.Bool, out _ledgeHangingAnimationParameter);
            RegisterAnimatorParameter(_ledgeClimbingAnimationParameterName, AnimatorControllerParameterType.Bool, out _ledgeClimbingAnimationParameter);
        }

        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ledgeHangingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<LedgeEvent>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<LedgeEvent>();
        }

        public override void ResetAbility()
        {
            base.ResetAbility();

            if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
            {
                DetachFromLedge();
            }

            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ledgeHangingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        }
    }
}