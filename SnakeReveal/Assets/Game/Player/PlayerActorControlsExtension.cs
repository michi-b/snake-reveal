using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public partial class PlayerActorControls : PlayerActorControls.IPlayerActorActions
    {
        private const float Threshold = 0.5f;
        private readonly List<GridDirection> _directionRequests = new(4);
        private readonly Action<GridDirection> _requestDirectionChange;

        public PlayerActorControls(Action<GridDirection> requestDirectionChange) : this()
        {
            _requestDirectionChange = requestDirectionChange;
            PlayerActor.SetCallbacks(this);
        }

        public void EvaluateDirectionRequests()
        {
            if(_directionRequests.Count == 0)
            {
                return;
            }

            _requestDirectionChange(_directionRequests[^1]);
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            RegisterDirectionInput(context, GridDirection.Right);
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            RegisterDirectionInput(context, GridDirection.Up);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            RegisterDirectionInput(context, GridDirection.Left);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            RegisterDirectionInput(context, GridDirection.Down);
        }

        private void RegisterDirectionInput(InputAction.CallbackContext context, GridDirection direction)
        {
            if (context.performed)
            {
                _directionRequests.Add(direction);
            }
            else if (context.canceled)
            {
                _directionRequests.Remove(direction);
            }
        }

        private enum DirectionRequest
        {
            Up,
            Right,
            Down,
            Left
        }
    }
}