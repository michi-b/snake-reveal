using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Settings;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Utility;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Game.Player.Controls.Touch
{
    public class SwipeEvaluation : IDisposable
    {
        private bool _isTracking;
        private readonly GameSettings _settings;
        private bool _disposed;
        private bool[] _hasFingerSwiped = Array.Empty<bool>();

        public SwipeEvaluation(GameSettings settings)
        {
            _settings = settings;
        }

        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                if (_disposed || value == _isTracking)
                {
                    return;
                }

                if (value)
                {
                    AssertIsNotDisposed();

                    EnhancedTouchSupport.Enable();

                    ReadOnlyArray<Finger> fingers = UnityEngine.InputSystem.EnhancedTouch.Touch.fingers;
                    Array.Resize(ref _hasFingerSwiped, fingers.Count);
                    for (int i = 0; i < fingers.Count; i++)
                    {
                        _hasFingerSwiped[i] = false;
                    }

                    UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += OnFingerUp;
                }
                else
                {
                    EnhancedTouchSupport.Disable();
                    UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= OnFingerUp;
                }

                _isTracking = value;
            }
        }

        public bool TryConsumeSwipe(GridDirections availableDirections, out GridDirection swipeDirection)
        {
            AssertIsNotDisposed();

            if (_isTracking && EnhancedTouchSupport.enabled)
            {
                foreach (UnityEngine.InputSystem.EnhancedTouch.Touch touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
                {
                    if (TryConsumeSwipe(touch, availableDirections, out swipeDirection))
                    {
                        return true;
                    }
                }
            }

            swipeDirection = GridDirection.None;
            return false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            IsTracking = false;
            _disposed = true;
        }

        private void AssertIsNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SwipeEvaluation));
            }
        }

        private void OnFingerUp(Finger finger)
        {
            _hasFingerSwiped[finger.index] = false;
        }

        private bool TryConsumeSwipe(UnityEngine.InputSystem.EnhancedTouch.Touch touch, GridDirections availableDirections, out GridDirection swipeDirection)
        {
            swipeDirection = GridDirection.None;

            if (!touch.valid || touch.phase != TouchPhase.Moved || _hasFingerSwiped[touch.finger.index])
            {
                return false;
            }

            Vector2 currentPosition;
            try
            {
                currentPosition = touch.screenPosition;
            }
            catch (Exception)
            {
                Debug.LogError("Can't get current touch screen position.");
                return false;
            }

            Vector2 startPosition;
            try
            {
                startPosition = touch.startScreenPosition;
            }
            catch (Exception)
            {
                Debug.LogError("Can't get current touch start screen position.");
                return false;
            }

            Vector2 pixelDelta = currentPosition - startPosition;
            Vector2 delta = pixelDelta / (ScreenUtility.Dpi * _settings.SwipeThreshold);

            if (TryGetSwipe(availableDirections, delta, out swipeDirection))
            {
                Debug.Log($"Accepted direction {{{swipeDirection}}} with delta {{{delta}}} from finger index {{{touch.finger.index}}}");
                _hasFingerSwiped[touch.finger.index] = true;
                return true;
            }

            return false;
        }

        private static bool TryGetSwipe(GridDirections availableDirections, Vector2 delta, out GridDirection swipeDirection)
        {
            float xDelta = delta.x;
            float yDelta = delta.y;
            float xDeltaAbs = Mathf.Abs(xDelta);
            float yDeltaAbs = Mathf.Abs(yDelta);

            if (xDeltaAbs > yDeltaAbs)
            {
                if (TryGetHorizontalSwipe(out swipeDirection))
                {
                    return true;
                }

                if (TryGetVerticalSwipe(out swipeDirection))
                {
                    return true;
                }
            }
            else
            {
                if (TryGetVerticalSwipe(out swipeDirection))
                {
                    return true;
                }

                if (TryGetHorizontalSwipe(out swipeDirection))
                {
                    return true;
                }
            }

            swipeDirection = GridDirection.None;
            return false;

            bool TryGetHorizontalSwipe(out GridDirection direction)
            {
                if (xDeltaAbs > 1)
                {
                    direction = xDelta > 0 ? GridDirection.Right : GridDirection.Left;
                    return availableDirections.Contains(direction);
                }

                direction = GridDirection.None;
                return false;
            }

            bool TryGetVerticalSwipe(out GridDirection direction)
            {
                if (yDeltaAbs > 1)
                {
                    direction = yDelta > 0 ? GridDirection.Up : GridDirection.Down;
                    return availableDirections.Contains(direction);
                }

                direction = GridDirection.None;
                return false;
            }
        }
    }
}