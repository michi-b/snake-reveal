using System;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public partial class @PlayerActorControls: IInputActionCollection2, IDisposable
    {
        private readonly PlayerActor _actor;
        
        public PlayerActorControls(PlayerActor actor) : this()
        {
            _actor = actor;
            PlayerActor.Down.performed += OnDownPerformed;
            PlayerActor.Up.performed += OnUpPerformed;
            PlayerActor.Left.performed += OnLeftPerformed;
            PlayerActor.Right.performed += OnRightPerformed;
            PlayerActor.Enable();
        }

        private void OnDownPerformed(InputAction.CallbackContext ctx)
        {
            _actor.Direction = GridDirection.Down;
        }

        private void OnUpPerformed(InputAction.CallbackContext ctx)
        {
            _actor.Direction = GridDirection.Up;
        }
        
        private void OnLeftPerformed(InputAction.CallbackContext ctx)
        {
            _actor.Direction = GridDirection.Left;
        }
        
        private void OnRightPerformed(InputAction.CallbackContext ctx)
        {
            _actor.Direction = GridDirection.Right;
        }
   }
}