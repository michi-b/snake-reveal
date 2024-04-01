﻿using System.Collections.Generic;
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
        private SwipeEvaluation _swipeEvaluation;

        private readonly List<GridDirection> _heldDirectionKeys = new(4);

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

        public void Activate()
        {
            Enable();
            _swipeEvaluation.IsTracking = true;
        }

        public void Deactivate()
        {
            _swipeEvaluation.IsTracking = false;
            Disable();
            _heldDirectionKeys.Clear();
        }

        public GridDirection GetDirectionChange(GridDirections availableDirections)
        {
            if (_swipeEvaluation.TryConsumeSwipe(availableDirections, out GridDirection swipeDirection))
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
            instance._swipeEvaluation = new SwipeEvaluation(debugInfoGui);
            instance.PlayerActor.SetCallbacks(instance);
            return instance;
        }

        public void Destroy()
        {
            Dispose();
            _swipeEvaluation.Dispose();
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