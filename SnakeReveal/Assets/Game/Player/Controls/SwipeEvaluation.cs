using Game.Gui.DebugInfo;
using UnityEngine;

namespace Game.Player.Controls
{
    public class SwipeEvaluation
    {
        private readonly DebugInfoGui _debugInfoGui;
        private bool _hasTouchEverMoved;
        private Vector2 _touchStartPosition;
        private Vector2 _touchCurrentPosition;
        private bool _isTouching;

        public SwipeEvaluation(DebugInfoGui debugInfoGui)
        {
            _debugInfoGui = debugInfoGui;
        }

        private bool IsTouching
        {
            get => _isTouching;
            set
            {
                _isTouching = value;
#if DEBUG
                _debugInfoGui.IsTouching = value;
#endif
            }
        }

        private Vector2 TouchStartPosition
        {
            get => _touchStartPosition;
            set
            {
                _touchStartPosition = value;
#if DEBUG
                _debugInfoGui.TouchStartPosition = value;
#endif
            }
        }

        public Vector2 TouchCurrentPosition
        {
            get => _touchCurrentPosition;
            set
            {
                _touchCurrentPosition = value;
#if DEBUG
                _debugInfoGui.TouchCurrentPosition = value;
#endif
            }
        }

        public void NotifyTouchStart(Vector2 position)
        {
            IsTouching = true;
            if (_hasTouchEverMoved)
            {
                TouchStartPosition = position;
            }
            else if (TouchStartPosition != Vector2.zero)
            {
                TouchStartPosition = position;
                _hasTouchEverMoved = true;
            }
        }

        public void NotifyTouchEnd(Vector2 position)
        {
            IsTouching = false;
        }

        public void NotifyTouchMove(Vector2 position)
        {
            if (!IsTouching)
            {
                return;
            }

            TouchCurrentPosition = position;

            if (!_hasTouchEverMoved)
            {
                // for some reason, at least with simulated touch input, the touch position on first contact is always initially (0, 0)
                // some users reported the same to happen on some phones
                TouchStartPosition = position;
                _hasTouchEverMoved = true;
            }
        }

        public void Clear()
        {
            _hasTouchEverMoved = false;
        }
    }
}