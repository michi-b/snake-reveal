using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Player.Controls.Touch.Extensions;
using Game.Settings;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Utility;

namespace Game.Player.Controls.Touch
{
    public class SwipeEvaluation : IDisposable
    {
        private readonly Vector2[] _swipeStarts;
        private bool _isTracking;
        private readonly GameSettings _settings;

        public SwipeEvaluation(GameSettings settings)
        {
            _settings = settings;
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
                    ResetSwipeStarts();
                }
            }
        }

        public Vector2 GetSwipeStart(int fingerIndex) => _swipeStarts[fingerIndex];

        private void OnFingerDown(Finger finger)
        {
            if (IsTracking)
            {
                Vector2 touchStartPosition = finger.currentTouch.startScreenPosition;
                _swipeStarts[finger.index] = touchStartPosition;
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

            Vector2 pixelDelta = finger.GetLatestScreenPosition() - _swipeStarts[finger.index];

            Vector2 delta = pixelDelta / (ScreenUtility.Dpi * _settings.SwipeThreshold);

            if (Mathf.Abs(delta.x) > 1)
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

            if (Mathf.Abs(delta.y) > 1)
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

            bool CanReturn(GridDirection direction) => availableDirections.Contains(direction);
        }

        public void Dispose()
        {
            UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
            EnhancedTouchSupport.Disable();
        }
    }
}