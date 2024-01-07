using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public partial class PlayerActorControls : PlayerActorControls.IPlayerActorActions
    {
        private const float Threshold = 0.5f;
        private readonly List<DirectionRequest> _directionRequests = new(4);

        public static PlayerActorControls Create()
        {
            var instance = new PlayerActorControls();
            instance.PlayerActor.SetCallbacks(instance);
            return instance;
        }

        public void Activate()
        {
            Enable();
            
            foreach (DirectionRequest directionRequest in DirectionRequestOptions)
            {
                if (GetIsDirectionRequestActive(directionRequest))
                {
                    _directionRequests.Add(directionRequest);
                }
            }
        }

        public void Deactivate()
        {
            Disable();
            
            _directionRequests.Clear();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            RegisterDirectionRequest(context, DirectionRequest.KeyRight);
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            RegisterDirectionRequest(context, DirectionRequest.KeyUp);
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            RegisterDirectionRequest(context, DirectionRequest.KeyLeft);
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            RegisterDirectionRequest(context, DirectionRequest.KeyDown);
        }

        public bool TryGetRequestedDirection(out GridDirection direction)
        {
            if (_directionRequests.Count == 0)
            {
                direction = GridDirection.None;
                return false;
            }

            DirectionRequest latestDirectionRequest = _directionRequests[^1];
            direction = latestDirectionRequest switch
            {
                DirectionRequest.KeyUp => GridDirection.Up,
                DirectionRequest.KeyRight => GridDirection.Right,
                DirectionRequest.KeyDown => GridDirection.Down,
                DirectionRequest.KeyLeft => GridDirection.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(latestDirectionRequest), latestDirectionRequest, null)
            };
            return true;
        }

        private void RegisterDirectionRequest(InputAction.CallbackContext context, DirectionRequest direction)
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
            KeyUp,
            KeyRight,
            KeyDown,
            KeyLeft
        }
        
        private static readonly DirectionRequest[] DirectionRequestOptions = new[]
        {
            DirectionRequest.KeyUp,
            DirectionRequest.KeyRight,
            DirectionRequest.KeyDown,
            DirectionRequest.KeyLeft
        };
        
        private InputAction GetInputAction(DirectionRequest directionRequest)
        {
            return directionRequest switch
            {
                DirectionRequest.KeyUp => m_PlayerActor_Up,
                DirectionRequest.KeyRight => m_PlayerActor_Right,
                DirectionRequest.KeyDown => m_PlayerActor_Down,
                DirectionRequest.KeyLeft => m_PlayerActor_Left,
                _ => throw new ArgumentOutOfRangeException(nameof(directionRequest), directionRequest, null)
            };
        }
        
        private bool GetIsDirectionRequestActive(DirectionRequest directionRequest)
        {
            return GetInputAction(directionRequest).ReadValue<float>() > Threshold;
        }
    }
}