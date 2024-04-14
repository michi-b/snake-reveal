using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Player.Controls.Touch.Extensions;
using Game.Settings;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Utility;

namespace Game.Player.Controls.Touch
{
    public class SwipeEvaluation : IDisposable
    {
        private FingerTouch[] _touches;
        private bool _isTracking;
        private readonly GameSettings _settings;
        private bool _disposed;

        public SwipeEvaluation(GameSettings settings)
        {
            _settings = settings;
        }

        public bool IsTracking
        {
            get => _isTracking;
            set
            {
                if (value != _isTracking)
                {
                    if (value)
                    {
                        AssertIsNotDisposed();

                        EnhancedTouchSupport.Enable();

                        ReadOnlyArray<Finger> fingers = UnityEngine.InputSystem.EnhancedTouch.Touch.fingers;
                        Array.Resize(ref _touches, fingers.Count);
                        for (int i = 0; i < fingers.Count; i++)
                        {
                            _touches[i] = new FingerTouch(fingers[i]);
                        }

                        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
                        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += OnFingerUp;
                    }
                    else
                    {
                        EnhancedTouchSupport.Disable();
                        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
                        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= OnFingerUp;
                    }

                    _isTracking = value;
                    foreach (Finger finger in UnityEngine.InputSystem.EnhancedTouch.Touch.fingers)
                    {
                        _touches[finger.index].Reset();
                    }
                }
            }
        }

        public bool TryGetTouch(int fingerIndex, out FingerTouch touch)
        {
            AssertIsNotDisposed();

            if (_touches == null || fingerIndex < 0 || fingerIndex >= _touches.Length)
            {
                touch = default;
                return false;
            }

            touch = _touches[fingerIndex];
            return true;
        }

        public bool TryConsumeSwipe(GridDirections availableDirections, out GridDirection swipeDirection)
        {
            AssertIsNotDisposed();

            if (_isTracking && EnhancedTouchSupport.enabled)
            {
                foreach (Finger finger in UnityEngine.InputSystem.EnhancedTouch.Touch.fingers)
                {
                    if (TryConsumeSwipe(finger, availableDirections, out swipeDirection))
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

        private void OnFingerDown(Finger finger)
        {
            _touches[finger.index].OnFingerDown();
        }

        private void OnFingerUp(Finger finger)
        {
            _touches[finger.index].OnFingerUp();
        }

        private bool TryConsumeSwipe(Finger finger, GridDirections availableDirections, out GridDirection swipeDirection)
        {
            FingerTouch fingerTouch = _touches[finger.index];

            if (!fingerTouch.IsTouching || fingerTouch.HasSwiped)
            {
                swipeDirection = GridDirection.None;
                return false;
            }

            Vector2 pixelDelta = finger.GetLatestScreenPosition() - fingerTouch.CurrentSwipeStart;

            Vector2 delta = pixelDelta / (ScreenUtility.Dpi * _settings.SwipeThreshold);

            if (TryGetSwipe(availableDirections, delta, out swipeDirection))
            {
                fingerTouch.ConsumeSwipe();
                return true;
            }

            swipeDirection = GridDirection.None;
            return false;
        }

        private static bool TryGetSwipe(GridDirections availableDirections, Vector2 delta, out GridDirection swipeDirection)
        {
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
    }
}