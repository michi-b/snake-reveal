using System;
using Extensions;
using Game.Enums;
using Game.Lines;
using Game.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Game
{
    public class DrawnShape : MonoBehaviour
    {
        [SerializeField] private LineLoop _lineLoop;
        [SerializeField] private SpriteShapeController _spriteShapeController;

        public Turn Turn => _lineLoop.Turn;

        public Line GetLine(Vector2Int position)
        {
            return _lineLoop.TryGetFirstLineAt(position, out Line line) ? line : throw new InvalidOperationException("Actor is not on shape");
        }

        public Turn GetTurn(bool startToEnd = true)
        {
            return _lineLoop.GetTurn(startToEnd);
        }

        public bool TryGetReconnectionLine(PlayerActor actor, out Line line)
        {
            // lines the player can collide with must be directed like the player direction, but turned left/right opposite of the shape turn
            GridDirection collisionLinesDirection = actor.Direction.Turn(Turn.Reverse());
            return _lineLoop.TryGetLineAt(actor.Position, collisionLinesDirection, out line);
        }

        public InsertionResult Insert(DrawingChain drawing, Line breakoutLine, Line reinsertionLine)
        {
            InsertionResult insertionResult = _lineLoop.Insert(drawing.Lines, breakoutLine, reinsertionLine);
            Apply();
            drawing.Deactivate();
            return insertionResult;
        }


        [ContextMenu(nameof(Apply))]
        private void EditModeApply()
        {
            Undo.RegisterCompleteObjectUndo(_spriteShapeController.gameObject, "Apply drawn shape");
            Apply();
        }

        private void Apply()
        {
            _spriteShapeController.spline.Clear();

            int currentIndex = 0;
            foreach (Line line in _lineLoop)
            {
                _spriteShapeController.spline.InsertPointAt(currentIndex++, line.Start.GetScenePosition(_lineLoop.Grid));
            }
        }
    }
}