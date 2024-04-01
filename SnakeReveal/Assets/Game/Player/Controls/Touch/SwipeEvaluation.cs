using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Gui.DebugInfo;
using Game.Player.Controls.Touch.Extensions;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls.Touch
{
    public class SwipeEvaluation : IDisposable
    {
        private const float SwipeThreshold = 50f;

        private readonly DebugInfoGui _debugInfoGui;
        private readonly Vector2[] _swipeStarts;
        private bool _isTracking;

        public SwipeEvaluation(DebugInfoGui debugInfoGui)
        {
            _debugInfoGui = debugInfoGui;
            EnhancedTouchSupport.Enable();
            _swipeStarts = new Vector2[UnityEngine.InputSystem.EnhancedTouch.Touch.fingers.Count];
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
        }

        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                if (value != _isTracking)
                {
                    _isTracking = value;
                    if (!_isTracking)
                    {
                        ResetSwipeStarts();
                    }
                }
            }
        }

        public void Dispose()
        {
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
            EnhancedTouchSupport.Disable();
        }

        private void OnFingerDown(Finger finger)
        {
            if (IsTracking)
            {
                Vector2 touchStartPosition = finger.currentTouch.startScreenPosition;
                _swipeStarts[finger.index] = touchStartPosition;
#if DEBUG
                if (finger.index == 0)
                {
                    _debugInfoGui.Swipe0Start = touchStartPosition;
                }
#endif
            }
        }

        public bool TryConsumeSwipe(GridDirections availableDirections, out GridDirection swipeDirection)
        {
            foreach (Finger finger in UnityEngine.InputSystem.EnhancedTouch.Touch.fingers)
            {
                if (TryGetSwipe(finger, availableDirections, out swipeDirection))
                {
                    ResetSwipeStarts();
                    return true;
                }
            }

            swipeDirection = GridDirection.None;
            return false;
        }

        private void ResetSwipeStarts()
        {
            foreach (Finger finger in UnityEngine.InputSystem.EnhancedTouch.Touch.fingers)
            {
                FingerTouchInteraction touchInteraction = finger.GetTouchInteraction();

                if (touchInteraction != FingerTouchInteraction.None)
                {
                    _swipeStarts[finger.index] = finger.GetLatestScreenPosition();
                }
            }
        }

        private bool TryGetSwipe(Finger finger, GridDirections availableDirections, out GridDirection swipeDirection)
        {
            FingerTouchInteraction touchInteraction = finger.GetTouchInteraction();

            if (touchInteraction == FingerTouchInteraction.None)
            {
                swipeDirection = GridDirection.None;
                return false;
            }

            Vector2 delta = finger.GetLatestScreenPosition() - _swipeStarts[finger.index];

            if (Mathf.Abs(delta.x) > SwipeThreshold)
            {
                if (delta.x > 0)
                {
                    if (CanReturn(GridDirection.Right))
                    {
                        swipeDirection = GridDirection.Right;
                        return true;
                    }
                }
                else if (CanReturn(GridDirection.Left))
                {
                    swipeDirection = GridDirection.Left;
                    return true;
                }
            }

            if (Mathf.Abs(delta.y) > SwipeThreshold)
            {
                if (delta.y > 0)
                {
                    if (CanReturn(GridDirection.Up))
                    {
                        swipeDirection = GridDirection.Up;
                        return true;
                    }
                }
                else if (CanReturn(GridDirection.Down))
                {
                    swipeDirection = GridDirection.Down;
                    return true;
                }
            }

            swipeDirection = GridDirection.None;
            return false;

            bool CanReturn(GridDirection direction)
            {
                Debug.Log($"Return direction {direction} by delta {delta}");
                return availableDirections.Contains(direction);
            }
        }
    }
}