using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls.Touch.Extensions
{
    public static class FingerExtensions
    {
        public static FingerTouchInteraction GetTouchInteraction(this Finger finger)
        {
            var result = FingerTouchInteraction.None;
            if (finger.currentTouch.valid)
            {
                result |= FingerTouchInteraction.IsTouching;
            }

            if (finger.lastTouch.valid)
            {
                result |= FingerTouchInteraction.HasTouched;
            }

            return result;
        }

        public static Vector2 GetLatestScreenPosition(this Finger finger)
        {
            return finger.GetTouchInteraction() switch
            {
                FingerTouchInteraction.IsTouching => finger.currentTouch.screenPosition,
                FingerTouchInteraction.HasTouched => finger.lastTouch.screenPosition,
                FingerTouchInteraction.HasTouchedAndIsTouching => finger.currentTouch.screenPosition,
                _ => default
            };
        }

        public static Vector2 GetLatestStartScreenPosition(this Finger finger)
        {
            return finger.GetTouchInteraction() switch
            {
                FingerTouchInteraction.IsTouching => finger.currentTouch.startScreenPosition,
                FingerTouchInteraction.HasTouched => finger.lastTouch.startScreenPosition,
                FingerTouchInteraction.HasTouchedAndIsTouching => finger.currentTouch.startScreenPosition,
                _ => default
            };
        }
    }
}