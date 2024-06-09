using Game.Player.Controls.Touch.Extensions;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls.Touch
{
    public class FingerTouch
    {
        public Finger Finger { get; }
        public bool IsTouching => Finger.currentTouch is { valid: true, isInProgress: true };

        public Vector2 ContactStart => IsTouching ? Finger.currentTouch.startScreenPosition : new Vector2(float.NaN, float.NaN);

        public bool HasSwiped { get; private set; }

        public FingerTouch(Finger finger)
        {
            Finger = finger;
            HasSwiped = false;
        }

        public void OnFingerDown()
        {
            if (!Finger.currentTouch.valid)
            {
                return;
            }

            HasSwiped = false;
        }

        public void OnFingerUp()
        {
            Reset();
        }

        public void ConsumeSwipe()
        {
            Debug.Assert(IsTouching);
            HasSwiped = true;
        }

        public void Reset()
        {
            HasSwiped = false;
        }

        public Vector2 GetLatestScreenPosition() => Finger.GetLatestScreenPosition();

        public Vector2 GetLatestStartScreenPosition() => Finger.GetLatestStartScreenPosition();
    }
}