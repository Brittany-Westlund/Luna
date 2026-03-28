using UnityEngine;
using System.Reflection;
using MoreMountains.CorgiEngine;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Ledge Climb Stabilizer")]
    public class CharacterLedgeClimbStabilizer : CharacterAbility
    {
        [Header("Behavior")]
        public bool LerpDuringClimb = true;
        public bool ForceGravityOffDuringClimb = true;
        public float ExtraHoldTime = 0f;

        [Header("Timing")]
        [Tooltip("How long to stay locked at the actual climb start position before moving upward.")]
        public float MoveStartDelay = 0.07f;

        [Tooltip("How long the move to ClimbOffset should take after the delay.")]
        public float MoveDuration = 0.03f;

        protected CharacterLedgeHang _ledgeHang;
        protected FieldInfo _ledgeField;

        protected Ledge _currentLedge;
        protected float _climbStartTime;
        protected bool _wasClimbing;

        // NEW: store the real starting position when climb begins
        protected Vector3 _actualClimbStartPosition;

        protected override void Initialization()
        {
            base.Initialization();

            _ledgeHang = _character?.FindAbility<CharacterLedgeHang>();
            if (_ledgeHang != null)
            {
                _ledgeField = typeof(CharacterLedgeHang).GetField("_ledge", BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();

            bool isClimbing = (_movement.CurrentState == CharacterStates.MovementStates.LedgeClimbing);

            if (isClimbing && !_wasClimbing)
            {
                _climbStartTime = Time.time;
                _currentLedge = GetCurrentLedge();

                // Capture the actual world position at the instant climbing begins
                _actualClimbStartPosition = _controller.transform.position;
            }

            if (isClimbing && _currentLedge != null)
            {
                if (ForceGravityOffDuringClimb)
                {
                    _controller.GravityActive(false);
                    _controller.SetForce(Vector2.zero);
                }

                Vector3 climbPos = _currentLedge.transform.position + _currentLedge.ClimbOffset;

                if (!LerpDuringClimb)
                {
                    _controller.transform.position = _actualClimbStartPosition;
                }
                else
                {
                    float elapsed = Time.time - _climbStartTime;

                    if (elapsed < MoveStartDelay)
                    {
                        // Hold exactly where Luna actually was when climb started
                        _controller.transform.position = _actualClimbStartPosition;
                    }
                    else
                    {
                        float duration = Mathf.Max(0.001f, MoveDuration);
                        float t = (elapsed - MoveStartDelay) / duration;
                        t = Mathf.Clamp01(t);

                        // Smoothstep easing
                        t = t * t * (3f - 2f * t);

                        _controller.transform.position = Vector3.Lerp(_actualClimbStartPosition, climbPos, t);
                    }
                }
            }

            if (!isClimbing && _wasClimbing && ExtraHoldTime > 0f && _currentLedge != null)
            {
                if (Time.time - _climbStartTime < (MoveStartDelay + MoveDuration + ExtraHoldTime))
                {
                    _controller.GravityActive(false);
                }
                else
                {
                    _currentLedge = null;
                }
            }
            else if (!isClimbing && _wasClimbing)
            {
                _currentLedge = null;
            }

            _wasClimbing = isClimbing;
        }

        protected Ledge GetCurrentLedge()
        {
            if (_ledgeHang == null || _ledgeField == null)
            {
                return null;
            }

            return _ledgeField.GetValue(_ledgeHang) as Ledge;
        }
    }
}