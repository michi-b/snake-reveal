using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        public const string LastFieldName = nameof(_end);

        [FormerlySerializedAs("_last"), SerializeField, HideInInspector]
        private Line _end;

        public override bool Loop => false;
        public new Line Start => base.Start;
        public override Line End => _end;

        protected override Color GizmosColor => new(1f, 0.5f, 0f);

        protected override void PostProcessEditModeLineChanges()
        {
            Line current = Start;
            while (current.Next != null)
            {
                current = current.Next;
            }

            _end = current;
            base.PostProcessEditModeLineChanges();
        }

        public void Clear()
        {
            if (_start != null)
            {
                foreach (Line line in this)
                {
                    LineCache.Return(line);
                }

                _start = _end = null;
            }
        }

        public void Set(Vector2Int start, Vector2Int end)
        {
            Clear();
            Line line = GetNewLine(start, end);
            _start = _end = line;
        }

        public void Extend(Vector2Int actorPosition)
        {
            if (!End.TryExtend(actorPosition))
            {
                Line newEnd = GetNewLine(End.End, actorPosition);

                newEnd.Previous = _end;
                End.Next = newEnd;

                _end = newEnd;
                End.Validate();
            }
        }
    }
}