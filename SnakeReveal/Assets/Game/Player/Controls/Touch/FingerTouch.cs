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
            Finger = finger;
            SetState(false, false, default);
        }

        public void OnFingerDown()
        {
            if (!Finger.currentTouch.valid)
            {
                return;
            }

            Vector2 touchStartPosition = Finger.currentTouch.startScreenPosition;
            
            SetState(true, false, touchStartPosition);
        }

        public void OnFingerUp()
        {
            Reset();
        }

        public void ConsumeSwipe()
        {
            
            Debug.Assert(IsTouching);
            SetState(IsTouching, true, default);
        }

        public void Reset()
        {
            SetState(false, false, default);
        }
        
        private void SetState(bool isTouching, bool hasSwiped, Vector2 currentSwipeStart)
        {
            IsTouching = isTouching;
            HasSwiped = hasSwiped;
            CurrentSwipeStart = currentSwipeStart;
        }

        public Vector2 GetLatestScreenPosition() => Finger.GetLatestScreenPosition();

        public Vector2 GetLatestStartScreenPosition() => Finger.GetLatestStartScreenPosition();
    }
}