using Game.Player.Controls.Touch.Extensions;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls.Touch
{
    public struct FingerTouch
    {
        public bool IsTouching;
        public bool HasSwiped;
        public Vector2 CurrentSwipeStart;

        public Finger Finger { get; private set; }

        public FingerTouch(Finger finger)
        {
            IsTouching = false;
            HasSwiped = false;
            CurrentSwipeStart = Vector2.zero;
            Finger = finger;
        }

        public void OnFingerDown()
        {
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