using System;
using Game.Enums;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public partial class PlayerActorControls
    {
        private readonly Action<GridDirection> _requestDirectionChange;

        public PlayerActorControls(Action<GridDirection> requestDirectionChange) : this()
        {
            _requestDirectionChange = requestDirectionChange;
            PlayerActor.Down.performed += OnDownPerformed;
            PlayerActor.Up.performed += OnUpPerformed;
            PlayerActor.Left.performed += OnLeftPerformed;
            PlayerActor.Right.performed += OnRightPerformed;
            PlayerActor.Enable();
        }

        private void OnDownPerformed(InputAction.CallbackContext ctx)
        {
            _requestDirectionChange(GridDirection.Down);
        }

        private void OnUpPerformed(InputAction.CallbackContext ctx)
        {
            _requestDirectionChange(GridDirection.Up);
        }

        private void OnLeftPerformed(InputAction.CallbackContext ctx)
        {
            _requestDirectionChange(GridDirection.Left);
        }

        private void OnRightPerformed(InputAction.CallbackContext ctx)
        {
            _requestDirectionChange(GridDirection.Right);
        }
    }
}