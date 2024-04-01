using System;
using Game.Gui.DebugInfo;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Game.Player.Controls
{
    public class SwipeEvaluation : IDisposable
    {
        private readonly DebugInfoGui _debugInfoGui;
        private readonly Vector2[] _swipeStarts;

        public SwipeEvaluation(DebugInfoGui debugInfoGui)
        {
            _debugInfoGui = debugInfoGui;
            EnhancedTouchSupport.Enable();
            _swipeStarts = new Vector2[Touch.fingers.Count];
            Touch.onFingerDown += OnFingerDown;
        }

        public void Dispose()
        {
            Touch.onFingerDown -= OnFingerDown;
            EnhancedTouchSupport.Disable();
        }

        private void OnFingerDown(Finger finger)
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
}