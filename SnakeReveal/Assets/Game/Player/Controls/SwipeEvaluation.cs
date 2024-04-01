using System;
using Game.Gui.DebugInfo;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls
{
    public class SwipeEvaluation : IDisposable
    {
        private readonly DebugInfoGui _debugInfoGui;

        public SwipeEvaluation()
        {
            EnhancedTouchSupport.Enable();
        }

        public void Dispose()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}