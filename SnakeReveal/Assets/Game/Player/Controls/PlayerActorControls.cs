using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Gui;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Game.Player.Controls
{
    public partial class PlayerActorControls : PlayerActorControls.IPlayerActorActions, IPlayerActorControls
    {
        private const float Threshold = 0.5f;

        private static readonly GridDirection[] DirectionKeys =
        {
            GridDirection.Up,
            GridDirection.Right,
            GridDirection.Down,
            GridDirection.Left
        };
        
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

        public void OnPrimaryTouchContact(InputAction.CallbackContext context)
        {
            var touchPosition = PlayerActor.PrimaryTouchPosition.ReadValue<Vector2>();
            
            if(context.started)
            {
                _swipeEvaluation.NotifyTouchStart(touchPosition);
            }
            else if(context.canceled)
            {
                _swipeEvaluation.NotifyTouchEnd(touchPosition);
            }
        }

        public void OnPrimaryTouchPosition(InputAction.CallbackContext context)
        {
            _swipeEvaluation.NotifyTouchMove(context.ReadValue<Vector2>());
         }

        public void Activate()
        {
            Enable();
        }

        public void Deactivate()
        {
            Disable();
            _heldDirectionKeys.Clear();
            _swipeEvaluation.Clear();
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

        public static PlayerActorControls Create(GuiContainer simulationGui)
        {
            TouchSimulation.Enable();
            var instance = new PlayerActorControls();
            instance._swipeEvaluation = new SwipeEvaluation(simulationGui.DebugInfo);
            instance.PlayerActor.SetCallbacks(instance);
            return instance;
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

        private InputAction GetInputAction(GridDirection directionKey)
        {
            return directionKey switch
            {
                GridDirection.Up => m_PlayerActor_Up,
                GridDirection.Right => m_PlayerActor_Right,
                GridDirection.Down => m_PlayerActor_Down,
                GridDirection.Left => m_PlayerActor_Left,
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(directionKey), directionKey, null),
                _ => throw new ArgumentOutOfRangeException(nameof(directionKey), directionKey, null)
            };
        }
    }
}