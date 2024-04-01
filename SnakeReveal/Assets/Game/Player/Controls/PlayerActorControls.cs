using System;
using System.Collections.Generic;
using Game.Enums;
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
        }

        public void Deactivate()
        {
            Disable();
            _heldDirectionKeys.Clear();
        }

        public GridDirection EvaluateRequestedDirection()
        {
            if (_heldDirectionKeys.Count == 0)
            {
                return GridDirection.None;
            }

            GridDirection latestDirectionKey = _heldDirectionKeys[^1];

            return latestDirectionKey switch
            {
                GridDirection.Up => GridDirection.Up,
                GridDirection.Right => GridDirection.Right,
                GridDirection.Down => GridDirection.Down,
                GridDirection.Left => GridDirection.Left,
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(latestDirectionKey), latestDirectionKey, null),
                _ => throw new ArgumentOutOfRangeException(nameof(latestDirectionKey), latestDirectionKey, null)
            };
        }

        public static PlayerActorControls Create()
        {
            TouchSimulation.Enable();
            var instance = new PlayerActorControls();
            instance._swipeEvaluation = new SwipeEvaluation();
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