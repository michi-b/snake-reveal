using Game.Player.Controls.Touch.Extensions;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls.Touch
{
    public class FingerTouch
    {
        private Finger Finger { get; }
        public bool IsTouching { get; private set; }
        public Vector2 CurrentSwipeStart { get; private set; }
        public bool HasSwiped { get; private set; }

        public FingerTouch(Finger finger)
        {
            IsTouching = false;
            HasSwiped = false;
            CurrentSwipeStart = Vector2.zero;
            Finger = finger;
        }

        public void OnFingerDown()
        {
            if (!Finger.currentTouch.valid)
            {
                return;
            }

            Vector2 touchStartPosition = Finger.currentTouch.startScreenPosition;
            CurrentSwipeStart = touchStartPosition;
            IsTouching = true;
        }

        public void OnFingerUp()
        {
            IsTouching = false;
            HasSwiped = false;
            CurrentSwipeStart = Vector2.zero;
        }

        public void ConsumeSwipe()
        {
            Debug.Assert(IsTouching);
            HasSwiped = true;
            CurrentSwipeStart = default;
        }

        public void Reset()
        {
            IsTouching = false;
            HasSwiped = false;
            CurrentSwipeStart = Vector2.zero;
        }

        public Vector2 GetLatestScreenPosition() => Finger.GetLatestScreenPosition();

        public Vector2 GetLatestStartScreenPosition() => Finger.GetLatestStartScreenPosition();
    }
}