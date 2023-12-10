﻿using System;
using Game.Enums;
using Game.Lines;
using Game.Player;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class PlayerDrawingSimulation
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;

        private readonly LineChain _drawingLineChain;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private readonly bool _isTravelingInShapeDirection;

        private readonly LineLoop _shape;


        private PlayerMovementMode _movementMode;
        private GridDirection _shapeTravelBreakoutDirection;

        private int2 _shapeTravelBreakoutPosition;
        [NotNull] private Line _shapeTravelLine;

        public PlayerDrawingSimulation(PlayerActor actor, LineLoop shape, LineChain drawingLineChain)
        {
            _actor = actor;
            _shape = shape;
            _drawingLineChain = drawingLineChain;
            _controls = new PlayerActorControls(RequestDirectionChange);
            _shapeTravelLine = _shape.FindLineAt(actor.Position, line => line.Direction.IsSameOrOpposite(actor.Direction));
            if (_shapeTravelLine == null)
            {
                throw new InvalidOperationException("Actor is not on shape");
            }

            _isTravelingInShapeDirection = _actor.Direction == _shapeTravelLine.Direction;
        }

        public bool ControlsEnabled
        {
            set
            {
                if (value)
                {
                    _controls.Enable();
                }
                else
                {
                    _controls.Disable();
                }
            }
        }

        public void Update()
        {
            for (int moveIndex = 0; moveIndex < _actor.Speed; moveIndex++)
            {
                switch (_movementMode)
                {
                    case PlayerMovementMode.ShapeTravel:
                        EvaluateShapeTraveling();
                        break;
                    case PlayerMovementMode.Drawing:
                        MovePlayerDrawing();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // todo: apply grid position only once per frame instead (and extrapolated)
            _actor.ApplyPosition();
        }

        private void RequestDirectionChange(GridDirection direction)
        {
            Turn turn = _actor.Direction.GetTurn(direction);
            if (turn == Turn.None)
            {
                return;
            }

            if (_movementMode switch
                {
                    PlayerMovementMode.ShapeTravel => GetIsValidTurnWhileShapeTraveling(turn),
                    PlayerMovementMode.Drawing => true,
                    _ => throw new ArgumentOutOfRangeException()
                })
            {
                _actor.Direction = direction;
            }
        }

        private bool GetIsValidTurnWhileShapeTraveling(Turn turn)
        {
            return _shape.Turn switch
            {
                Turn.None => throw GetHasNoShapeTravelTurnException(),
                Turn.Clockwise => turn == Turn.CounterClockwise,
                Turn.CounterClockwise => turn == Turn.Clockwise,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static Exception GetHasNoShapeTravelTurnException()
        {
            return new InvalidOperationException($"Shape travel turn is {nameof(Turn.None)} although it should never have that value by design");
        }

        private void EvaluateShapeTraveling()
        {
            Debug.Assert(_shapeTravelLine.Contains(_actor.Position));

            if (_actor.Direction == _shapeTravelLine.GetDirection(_isTravelingInShapeDirection))
            {
                // evaluate whether current travel line and consequently the current player direction need to be adjusted
                ContinueShapeTraveling();
            }
            else
            {
                DiscontinueShapeTraveling();
            }
        }

        private void ContinueShapeTraveling()
        {
            bool isAtLineEnd = _actor.Position.Equals(_shapeTravelLine.GetEnd(_isTravelingInShapeDirection));

            // if at line end, switch to next line and adjust direction
            if (isAtLineEnd)
            {
                Line newLine = _shapeTravelLine.GetNext(_isTravelingInShapeDirection);
                if (newLine == null)
                {
                    throw new InvalidOperationException("Line loop is not closed");
                }

                _shapeTravelLine = newLine;
                Debug.Assert(_shapeTravelLine != null, nameof(_shapeTravelLine) + " != null");
                GridDirection currentLineDirection = _shapeTravelLine.GetDirection(_isTravelingInShapeDirection);
                _actor.Direction = currentLineDirection;
            }

            _actor.Step();
        }

        private void DiscontinueShapeTraveling()
        {
#if DEBUG
            {
                Turn turn = _shapeTravelLine.GetDirection(_isTravelingInShapeDirection).GetTurn(_actor.Direction);
                Debug.Assert(GetIsValidTurnWhileShapeTraveling(turn));
            }
#endif
            _shapeTravelBreakoutPosition = _actor.Position;
            _movementMode = PlayerMovementMode.Drawing;
            Debug.Assert(_shapeTravelLine.Contains(_shapeTravelBreakoutPosition));
            _actor.Step();
            Debug.Assert(!_shapeTravelLine.Contains(_actor.Position));
            _drawingLineChain.Set(_shapeTravelBreakoutPosition, _actor.Position);
        }

        private void MovePlayerDrawing()
        {
            _actor.Step();
            _drawingLineChain.Extend(_actor.Position);
        }
    }
}