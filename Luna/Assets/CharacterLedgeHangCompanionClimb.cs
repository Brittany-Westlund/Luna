using UnityEngine;
using System.Reflection;
using MoreMountains.CorgiEngine;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Ledge Climb Stabilizer")]
    public class CharacterLedgeClimbStabilizer : CharacterAbility
    {
        [Header("Behavior")]
        [Tooltip("If true, smoothly moves from HangOffset to ClimbOffset during climb. If false, holds at HangOffset until teleport.")]
        public bool LerpDuringClimb = true;

        [Tooltip("If true, forces gravity off during LedgeClimbing.")]
        public bool ForceGravityOffDuringClimb = true;

        [Tooltip("Optional extra padding time to keep stabilizing after climb starts.")]
        public float ExtraHoldTime = 0f;

        protected CharacterLedgeHang _ledgeHang;
        protected FieldInfo _ledgeField;

        protected Ledge _currentLedge;
        protected float _climbStartTime;
        protected bool _wasClimbing;

        protected override void Initialization()
        {
            base.Initialization();

            _ledgeHang = _character?.FindAbility<CharacterLedgeHang>();
            if (_ledgeHang != null)
            {
                // CharacterLedgeHang has: protected Ledge _ledge;
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
            }

            if (isClimbing && _currentLedge != null)
            {
                if (ForceGravityOffDuringClimb)
                {
                    _controller.GravityActive(false);
                    _controller.SetForce(Vector2.zero);
                }

                Vector3 hangPos = _currentLedge.transform.position + _currentLedge.HangOffset;
                Vector3 climbPos = _currentLedge.transform.position + _currentLedge.ClimbOffset;

                if (!LerpDuringClimb)
                {
                    _controller.transform.position = hangPos;
                }
                else
                {
                    float duration = (_ledgeHang != null) ? _ledgeHang.ClimbingAnimationDuration : 0.5f;
                    duration = Mathf.Max(0.01f, duration);

                    float t = (Time.time - _climbStartTime) / duration;
                    t = Mathf.Clamp01(t);

                    _controller.transform.position = Vector3.Lerp(hangPos, climbPos, t);
                }
            }

            // optional: continue holding for a tiny beat after climb state starts
            if (!isClimbing && _wasClimbing && ExtraHoldTime > 0f && _currentLedge != null)
            {
                if (Time.time - _climbStartTime < ((_ledgeHang != null ? _ledgeHang.ClimbingAnimationDuration : 0.5f) + ExtraHoldTime))
                {
                    // keep gravity off briefly (MM will re-enable gravity after teleport/detach)
                    _controller.GravityActive(false);
                }
                else
                {
                    _currentLedge = null;
                }
            }

            _wasClimbing = isClimbing;
        }

        protected Ledge GetCurrentLedge()
        {
            if (_ledgeHang == null || _ledgeField == null) { return null; }
            return _ledgeField.GetValue(_ledgeHang) as Ledge;
        }
    }
}