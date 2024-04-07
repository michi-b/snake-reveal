using System.Collections.Generic;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Gui.DebugInfo;
using Game.Player.Controls.Touch;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls
{
    public partial class PlayerActorControls : PlayerActorControls.IPlayerActorActions, IPlayerActorControls
    {
        private readonly List<GridDirection> _heldDirectionKeys = new(4);
        
        private bool _isEnabled;

        public SwipeEvaluation SwipeEvaluation { get; private set; }

        public void OnRight(InputAction.CallbackContext context)
        {
            RegisterDirectionKey(context, GridDirection.Right);
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            RegisterDirectionKey(context, GridDirection.Up);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            RegisterDirectionKey(context, GridDirection.Left);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            RegisterDirectionKey(context, GridDirection.Down);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    SwipeEvaluation.IsTracking = value;
                    if (value)
                    {
                        Enable();
                    }
                    else
                    {
                        Disable();
                        _heldDirectionKeys.Clear();
                    }
                    _isEnabled = value;
                }
            }
        }

        public GridDirection GetDirectionChange(GridDirections availableDirections)
        {
            if (SwipeEvaluation.TryConsumeSwipe(availableDirections, out GridDirection swipeDirection))
            {
                return swipeDirection;
            }

            // get first available direction from key presses
            for (int i = _heldDirectionKeys.Count - 1; i >= 0; i--)
            {
                if (availableDirections.Contains(_heldDirectionKeys[i]))
                {
                    return _heldDirectionKeys[i];
                }
            }

            // if no requested direction is available, return none
            return GridDirection.None;
        }

        public static PlayerActorControls Create(DebugInfoGui debugInfoGui)
        {
            TouchSimulation.Enable();
            var instance = new PlayerActorControls();
            instance.SwipeEvaluation = new SwipeEvaluation();
            instance.PlayerActor.SetCallbacks(instance);
            return instance;
        }

        public void Destroy()
        {
            Dispose();
            SwipeEvaluation.Dispose();
        }

        private void RegisterDirectionKey(InputAction.CallbackContext context, GridDirection direction)
        {
            if (context.performed)
            {
                _heldDirectionKeys.Add(direction);
            }
            else if (context.canceled)
            {
                _heldDirectionKeys.Remove(direction);
            }
        }
    }
}